using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.ExternalServices.Email;
using Intervu.Application.Interfaces.Services;
using Intervu.Application.Interfaces.UseCases.Availability;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;
using Intervu.Application.Interfaces.UseCases.Notification;
using Intervu.Application.Utils;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace Intervu.Application.UseCases.InterviewBooking
{
    public class PayoutForCoachAfterInterview : IPayoutForCoachAfterInterview
    {
        private readonly IInterviewRoomRepository _interviewRoomRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICoachProfileRepository _coachProfileRepository;
        private readonly IBookingRequestRepository _bookingRequestRepository;
        private readonly IInterviewRoundRepository _interviewRoundRepository;
        private readonly ICommissionCalculator _commissionCalculator;
        private readonly IBackgroundService _jobService;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;

        public PayoutForCoachAfterInterview(
            IInterviewRoomRepository interviewRoomRepository,
            ITransactionRepository transactionRepository,
            ICoachProfileRepository coachProfileRepository,
            IBookingRequestRepository bookingRequestRepository,
            IInterviewRoundRepository interviewRoundRepository,
            ICommissionCalculator commissionCalculator,
            IBackgroundService jobService,
            IUserRepository userRepository,
            IConfiguration configuration,
            IUnitOfWork unitOfWork)
        {
            _interviewRoomRepository = interviewRoomRepository;
            _transactionRepository = transactionRepository;
            _coachProfileRepository = coachProfileRepository;
            _bookingRequestRepository = bookingRequestRepository;
            _interviewRoundRepository = interviewRoundRepository;
            _commissionCalculator = commissionCalculator;
            _jobService = jobService;
            _userRepository = userRepository;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
        }

        public async Task ExecuteAsync(Guid interviewRoomId)
        {
            var room = await _interviewRoomRepository.GetByIdWithDetailsAsync(interviewRoomId);
            if (room == null) return;

            var interviewerId = room.CoachId ?? throw new Exception("InterviewerId is missing for room");

            if (room.BookingRequestId == null) return;

            var round = await _interviewRoundRepository.GetByInterviewRoomIdAsync(interviewRoomId);
            if (round == null) return;

            // Idempotency: if a Payout row for this round already exists, this room was already paid out.
            var existing = await _transactionRepository.GetByInterviewRoundId(round.Id, TransactionType.Payout);
            if (existing != null) return;

            if (round.Price <= 0) return;

            var split = await _commissionCalculator.ComputeAsync(round.Price);
            if (split.Net <= 0) return;

            var bookingRequest = await _bookingRequestRepository.GetByIdWithDetailsAsync(room.BookingRequestId.Value);

            // Credit earnings to coach's internal balance with optimistic concurrency.
            // The Payout + Earnings rows are created inside the same transaction so the
            // existence of the Payout row is the durable idempotency marker.
            const int maxRetries = 3;
            var committed = false;
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    await _unitOfWork.BeginTransactionAsync();

                    var coach = await _coachProfileRepository.GetProfileByIdAsync(interviewerId);
                    coach.CurrentAmount = (coach.CurrentAmount ?? 0) + split.Net;
                    coach.Version++;
                    await _coachProfileRepository.UpdateCoachProfileAsync(coach);

                    var payoutTransaction = new InterviewBookingTransaction
                    {
                        Id = Guid.NewGuid(),
                        OrderCode = RandomGenerator.GenerateOrderCode(),
                        UserId = interviewerId,
                        BookingRequestId = room.BookingRequestId,
                        InterviewRoundId = round.Id,
                        Amount = split.Net,
                        GrossAmount = round.Price,
                        CommissionAmount = split.Commission,
                        CommissionRate = split.Rate,
                        Type = TransactionType.Payout,
                        Status = TransactionStatus.Paid,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _transactionRepository.AddAsync(payoutTransaction);

                    var earningsTransaction = new InterviewBookingTransaction
                    {
                        Id = Guid.NewGuid(),
                        OrderCode = RandomGenerator.GenerateOrderCode(),
                        UserId = interviewerId,
                        BookingRequestId = room.BookingRequestId,
                        InterviewRoundId = round.Id,
                        Amount = split.Net,
                        GrossAmount = round.Price,
                        CommissionAmount = split.Commission,
                        CommissionRate = split.Rate,
                        Type = TransactionType.Earnings,
                        Status = TransactionStatus.Paid,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _transactionRepository.AddAsync(earningsTransaction);

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();
                    committed = true;
                    break;
                }
                catch (Exception ex) when (attempt < maxRetries - 1 && _unitOfWork.IsConcurrencyException(ex))
                {
                    await _unitOfWork.RollbackTransactionAsync();
                }
            }

            if (!committed) return;

            var candidateName = bookingRequest?.Candidate?.User?.FullName;
            if (string.IsNullOrWhiteSpace(candidateName) && room.CandidateId.HasValue)
            {
                var candidateUser = await _userRepository.GetByIdAsync(room.CandidateId.Value);
                candidateName = candidateUser?.FullName;
            }

            if (string.IsNullOrWhiteSpace(candidateName))
            {
                candidateName = "the candidate";
            }

            var scheduledTime = room.ScheduledTime ?? round.StartTime;
            var interviewDateText = scheduledTime.ToLocalTime().ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

            var notificationMessage = $"Your payout for interview with {candidateName} at {interviewDateText} has been processed.";
            const string actionUrl = "/payment-history";
            _jobService.Enqueue<INotificationUseCase>(uc => uc.CreateAsync(
                interviewerId,
                NotificationType.PaymentSuccess,
                "Payout Processed",
                notificationMessage,
                actionUrl,
                null
            ));

            var coachUser = await _userRepository.GetByIdAsync(interviewerId);
            if (coachUser != null)
            {
                var frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:5173";
                var placeholders = new Dictionary<string, string>
                {
                    ["CoachName"] = coachUser.FullName,
                    ["Amount"] = split.Net.ToString("N0"),
                    ["DashboardLink"] = $"{frontendUrl.TrimEnd('/')}/payment-history"
                };

                _jobService.Enqueue<IEmailService>(svc => svc.SendEmailWithTemplateAsync(
                    coachUser.Email,
                    "PayoutConfirmation",
                    placeholders));
            }
        }
    }
}

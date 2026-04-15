using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.ExternalServices.Email;
using Intervu.Application.Interfaces.UseCases.Availability;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;
using Intervu.Application.Interfaces.UseCases.Notification;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Intervu.Application.UseCases.InterviewBooking
{
    public class PayoutForCoachAfterInterview : IPayoutForCoachAfterInterview
    {
        private readonly IInterviewRoomRepository _interviewRoomRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICoachProfileRepository _coachProfileRepository;
        private readonly IBackgroundService _jobService;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;

        public PayoutForCoachAfterInterview(
            IInterviewRoomRepository interviewRoomRepository,
            ITransactionRepository transactionRepository,
            ICoachProfileRepository coachProfileRepository,
            IBackgroundService jobService,
            IUserRepository userRepository,
            IConfiguration configuration,
            IUnitOfWork unitOfWork)
        {
            _interviewRoomRepository = interviewRoomRepository;
            _transactionRepository = transactionRepository;
            _coachProfileRepository = coachProfileRepository;
            _jobService = jobService;
            _userRepository = userRepository;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
        }

        public async Task ExecuteAsync(Guid interviewRoomId)
        {
            var room = await _interviewRoomRepository.GetByIdAsync(interviewRoomId);

            var interviewerId = room.CoachId ?? throw new Exception("InterviewerId is missing for room");
            var coach = await _coachProfileRepository.GetProfileByIdAsync(interviewerId);

            if (room.BookingRequestId == null) return;

            // Find payout transaction via BookingRequest
            InterviewBookingTransaction? t = await _transactionRepository.GetByBookingRequestId(room.BookingRequestId.Value, TransactionType.Payout);

            if (t == null) return;

            if (t.Status == TransactionStatus.Created)
            {
                // Skip payouts with non-positive amount
                if (t.Amount <= 0) return;

                // Credit earnings to coach's internal balance with optimistic concurrency
                const int maxRetries = 3;
                for (int attempt = 0; attempt < maxRetries; attempt++)
                {
                    try
                    {
                        await _unitOfWork.BeginTransactionAsync();

                        // Reload coach profile to get fresh state
                        coach = await _coachProfileRepository.GetProfileByIdAsync(interviewerId);

                        coach.CurrentAmount = (coach.CurrentAmount ?? 0) + t.Amount;
                        coach.Version++;
                        await _coachProfileRepository.UpdateCoachProfileAsync(coach);

                        // Create earnings transaction record
                        var earningsTransaction = new InterviewBookingTransaction
                        {
                            Id = Guid.NewGuid(),
                            UserId = interviewerId,
                            BookingRequestId = room.BookingRequestId,
                            Amount = t.Amount,
                            Type = TransactionType.Earnings,
                            Status = TransactionStatus.Paid,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _transactionRepository.AddAsync(earningsTransaction);

                        // Mark original payout transaction as processed
                        t.Status = TransactionStatus.Paid;
                        _transactionRepository.UpdateAsync(t);

                        await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitTransactionAsync();
                        break; // success
                    }
                    catch (DbUpdateConcurrencyException) when (attempt < maxRetries - 1)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        // Retry with fresh entity state
                    }
                }
            }

            var amount = t.Amount;
            _jobService.Enqueue<INotificationUseCase>(uc => uc.CreateAsync(
                interviewerId,
                NotificationType.PaymentSuccess,
                "Payout Processed",
                $"Your payout of {amount:N0} resources has been processed.",
                "/dashboard/wallet",
                null
            ));

            var coachUser = await _userRepository.GetByIdAsync(interviewerId);
            if (coachUser != null)
            {
                var frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:5173";
                var placeholders = new Dictionary<string, string>
                {
                    ["CoachName"] = coachUser.FullName,
                    ["Amount"] = amount.ToString("N0"),
                    ["DashboardLink"] = $"{frontendUrl.TrimEnd('/')}/dashboard/wallet"
                };

                _jobService.Enqueue<IEmailService>(svc => svc.SendEmailWithTemplateAsync(
                    coachUser.Email,
                    "PayoutConfirmation",
                    placeholders));
            }
        }
    }
}

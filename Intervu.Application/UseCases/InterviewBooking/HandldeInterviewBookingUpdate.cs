using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.ExternalServices.Email;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Application.Interfaces.UseCases.Notification;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Intervu.Application.UseCases.InterviewBooking
{
    public class HandldeInterviewBookingUpdate : IHandldeInterviewBookingUpdate
    {
        private readonly IPaymentService _paymentService;
        private readonly IBackgroundService _backgroundService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICoachInterviewServiceRepository _coachInterviewServiceRepository;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<HandldeInterviewBookingUpdate> _logger;

        public HandldeInterviewBookingUpdate(
            IPaymentService paymentService,
            IBackgroundService backgroundService,
            IUnitOfWork unitOfWork,
            ICoachInterviewServiceRepository coachInterviewServiceRepository,
            IUserRepository userRepository,
            IConfiguration configuration,
            ILogger<HandldeInterviewBookingUpdate> logger)
        {
            _paymentService = paymentService;
            _backgroundService = backgroundService;
            _unitOfWork = unitOfWork;
            _coachInterviewServiceRepository = coachInterviewServiceRepository;
            _userRepository = userRepository;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task ExecuteAsync(object webhookPayload)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var (isValid, orderCode) = _paymentService.VerifyPayment(webhookPayload);

                if (!isValid) return;

                var transactionRepo = _unitOfWork.GetRepository<ITransactionRepository>();

                InterviewBookingTransaction transaction = await transactionRepo.Get(orderCode, TransactionType.Payment)
                    ?? throw new NotFoundException("Booking transaction not found");

                if (transaction.Status == TransactionStatus.Paid)
                {
                    _logger.LogInformation(
                        "Payment webhook already handled for booking transaction {TransactionId}",
                        transaction.Id);
                    return;
                }

                transaction.Status = TransactionStatus.Paid;

                if (transaction.BookingRequestId == null)
                    throw new NotFoundException("Transaction has no associated booking request");

                await HandleBookingRequestPayment(transaction);

                var candidateId = transaction.UserId;
                var coachId = transaction.CoachId;
                var frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:5173";

                DateTime? scheduledAt = transaction.BookedStartTime;
                int? durationMinutes = transaction.BookedDurationMinutes;
                if (transaction.BookingRequestId.HasValue)
                {
                    var bookingRequestRepo = _unitOfWork.GetRepository<IBookingRequestRepository>();
                    var bookingRequest = await bookingRequestRepo.GetByIdWithDetailsAsync(transaction.BookingRequestId.Value);
                    if (bookingRequest != null)
                    {
                        scheduledAt ??= bookingRequest.RequestedStartTime ?? bookingRequest.Rounds.OrderBy(r => r.RoundNumber).FirstOrDefault()?.StartTime;
                        durationMinutes ??= bookingRequest.CoachInterviewService?.DurationMinutes
                            ?? (bookingRequest.Rounds.Count > 0 ? (int)(bookingRequest.Rounds.First().EndTime - bookingRequest.Rounds.First().StartTime).TotalMinutes : null);
                    }
                }

                // Notify candidate — booking confirmed
                _backgroundService.Enqueue<INotificationUseCase>(
                    uc => uc.CreateAsync(
                        candidateId,
                        NotificationType.BookingAccepted,
                        "Booking confirmed",
                        "Your interview has been booked successfully.",
                        "/interview?tab=upcoming",
                        null));

                var bookingRepo = _unitOfWork.GetRepository<IBookingRequestRepository>();
                var bookingRequest = await bookingRepo.GetByIdAsync(transaction.BookingRequestId.Value);
                if (bookingRequest != null)
                {
                    var coachId = bookingRequest.CoachId;
                    // Notify coach — new booking
                    _backgroundService.Enqueue<INotificationUseCase>(
                        uc => uc.CreateAsync(
                            coachId,
                            NotificationType.BookingNew,
                            "New interview booking",
                            "A candidate has booked an interview with you.",
                            "/interview?tab=upcoming",
                            null));
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                try
                {
                    var candidate = await _userRepository.GetByIdAsync(candidateId);
                    if (candidate != null)
                    {
                        var receiptPlaceholders = new Dictionary<string, string>
                        {
                            ["CandidateName"] = candidate.FullName,
                            ["CoachName"] = "Coach",
                            ["Amount"] = transaction.Amount.ToString("N0"),
                            ["OrderCode"] = transaction.OrderCode.ToString(),
                            ["InterviewDate"] = (scheduledAt ?? DateTime.UtcNow).ToString("dd MMM yyyy"),
                            ["InterviewTime"] = (scheduledAt ?? DateTime.UtcNow).ToString("HH:mm"),
                            ["Duration"] = (durationMinutes ?? 60).ToString()
                        };

                        if (coachId.HasValue)
                        {
                            var coachUser = await _userRepository.GetByIdAsync(coachId.Value);
                            receiptPlaceholders["CoachName"] = coachUser?.FullName ?? "Coach";

                            if (coachUser != null)
                            {
                                var coachPlaceholders = new Dictionary<string, string>
                                {
                                    ["CoachName"] = coachUser.FullName,
                                    ["CandidateName"] = candidate.FullName,
                                    ["InterviewDate"] = (scheduledAt ?? DateTime.UtcNow).ToString("dd MMM yyyy"),
                                    ["InterviewTime"] = (scheduledAt ?? DateTime.UtcNow).ToString("HH:mm"),
                                    ["Duration"] = (durationMinutes ?? 60).ToString(),
                                    ["DashboardLink"] = $"{frontendUrl.TrimEnd('/')}/dashboard/interviews"
                                };

                                _backgroundService.Enqueue<IEmailService>(svc => svc.SendEmailWithTemplateAsync(
                                    coachUser.Email,
                                    "BookingConfirmationCoach",
                                    coachPlaceholders));
                            }
                        }

                        _backgroundService.Enqueue<IEmailService>(svc => svc.SendEmailWithTemplateAsync(
                            candidate.Email,
                            "PaymentReceipt",
                            receiptPlaceholders));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to enqueue payment receipt emails for transaction {TransactionId}", transaction.Id);
                }
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        /// <summary>
        /// Unified payment handler for all booking request types (Direct, External, JD).
        /// Marks booking as Paid and creates interview rooms from rounds.
        /// </summary>
        private async Task HandleBookingRequestPayment(InterviewBookingTransaction transaction)
        {
            var bookingRepo = _unitOfWork.GetRepository<IBookingRequestRepository>();

            var bookingRequest = await bookingRepo.GetByIdWithDetailsAsync(transaction.BookingRequestId!.Value)
                ?? throw new NotFoundException("Booking request not found");

            if (bookingRequest.Status != BookingRequestStatus.Accepted)
            {
                _logger.LogWarning(
                    "BookingRequest {Id} is not in Accepted status (current: {Status}), skipping room creation",
                    bookingRequest.Id, bookingRequest.Status);
                return;
            }

            // Transition to Paid
            bookingRequest.Status = BookingRequestStatus.Paid;
            bookingRequest.UpdatedAt = DateTime.UtcNow;
            bookingRepo.UpdateAsync(bookingRequest);

            // Create rooms from rounds (works for Direct with 1 round, JD with N rounds)
            foreach (var round in bookingRequest.Rounds.OrderBy(r => r.RoundNumber))
            {
                var roundDuration = round.CoachInterviewService?.DurationMinutes ?? 60;

                // Use the first availability block of this round as the reference
                var firstBlockId = round.AvailabilityBlocks?.OrderBy(b => b.StartTime).FirstOrDefault()?.Id;

                var room = new Domain.Entities.InterviewRoom
                {
                    CandidateId = bookingRequest.CandidateId,
                    CoachId = bookingRequest.CoachId,
                    ScheduledTime = round.StartTime,
                    DurationMinutes = roundDuration,
                    CurrentAvailabilityId = firstBlockId,
                    Status = InterviewRoomStatus.Scheduled,
                    TransactionId = transaction.Id,
                    BookingRequestId = bookingRequest.Id,
                    CoachInterviewServiceId = round.CoachInterviewServiceId,
                    AimLevel = bookingRequest.AimLevel,
                    RoundNumber = round.RoundNumber,
                    EvaluationResults = await CreateEvaluationResultsFromInterviewService(round.CoachInterviewServiceId),
                    IsEvaluationCompleted = false
                };
                _backgroundService.Enqueue<ICreateInterviewRoom>(uc => uc.ExecuteAsync(room));
            }

            _logger.LogInformation(
                "Queued {RoundCount} interview room(s) for BookingRequest {BookingRequestId} (Type: {Type})",
                bookingRequest.Rounds.Count, bookingRequest.Id, bookingRequest.Type);
        }

        private async Task<List<EvaluationResult>> CreateEvaluationResultsFromInterviewService(Guid? coachInterviewServiceId)
        {
            if (coachInterviewServiceId == null)
                return [];

            var service = await _coachInterviewServiceRepository.GetByIdWithDetailsAsync(coachInterviewServiceId.Value);

            if (service?.InterviewType?.EvaluationStructure == null)
                return [];

            return [.. service.InterviewType.EvaluationStructure.Select(c => new EvaluationResult
            {
                Type = c.Type,
                Question = c.Question,
                Score = 0,
                Answer = ""
            })];
        }
    }
}

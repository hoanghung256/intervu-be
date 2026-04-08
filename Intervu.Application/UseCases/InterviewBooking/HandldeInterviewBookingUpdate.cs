using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.ExternalServices.Email;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Application.Services;
using Intervu.Application.Interfaces.UseCases.Notification;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;

namespace Intervu.Application.UseCases.InterviewBooking
{
    public class HandldeInterviewBookingUpdate : IHandldeInterviewBookingUpdate
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IPaymentService _paymentService;
        private readonly ICoachAvailabilitiesRepository _coachAvailabilitiesRepository;
        private readonly IBackgroundService _backgroundService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICoachInterviewServiceRepository _coachInterviewServiceRepository;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<HandldeInterviewBookingUpdate> _logger;

        public HandldeInterviewBookingUpdate(
            ITransactionRepository transactionRepository,
            IPaymentService paymentService,
            ICoachAvailabilitiesRepository coachAvailabilitiesRepository,
            IBackgroundService backgroundService,
            IUnitOfWork unitOfWork,
            ICoachInterviewServiceRepository coachInterviewServiceRepository,
            IUserRepository userRepository,
            IConfiguration configuration,
            ILogger<HandldeInterviewBookingUpdate> logger)
        {
            _transactionRepository = transactionRepository;
            _paymentService = paymentService;
            _coachAvailabilitiesRepository = coachAvailabilitiesRepository;
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

                // --- Flow B/C: BookingRequest payment (Multiple rounds) ---
                if (transaction.BookingRequestId != null)
                {
                    await HandleBookingRequestPayment(transaction);
                }
                // --- Flow A: Normal availability booking (1 round) ---
                else if (transaction.CoachAvailabilityId != null)
                {
                    await HandleAvailabilityPayment(transaction);
                }
                else
                {
                    throw new NotFoundException("Transaction has no associated booking context");
                }

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

                if (coachId != null)
                {
                    var validCoachId = coachId.Value;
                    // Notify coach — new booking
                    _backgroundService.Enqueue<INotificationUseCase>(
                        uc => uc.CreateAsync(
                            validCoachId,
                            NotificationType.BookingNew,
                            "New interview booking",
                            "A candidate has booked an interview with you.",
                            "/interview?tab=upcoming",
                            null));
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

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
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

        }

        /// <summary>
        /// Flow A: Normal booking via coach availability slot.
        /// The availability was already split at checkout creation time.
        /// Here we just create the interview room using stored metadata.
        /// </summary>
        private async Task HandleAvailabilityPayment(InterviewBookingTransaction transaction)
        {
            var candidateId = transaction.UserId;
            var coachId = transaction.CoachId
                ?? throw new NotFoundException("Transaction is missing CoachId metadata");
            var startTime = transaction.BookedStartTime
                ?? throw new NotFoundException("Transaction is missing BookedStartTime metadata");
            var duration = transaction.BookedDurationMinutes
                ?? throw new NotFoundException("Transaction is missing BookedDurationMinutes metadata");

            // Updated for IC-127: Mark the associated BookingRequest as Paid
            if (transaction.BookingRequestId != null)
            {
                var brRepo = _unitOfWork.GetRepository<IBookingRequestRepository>();
                var br = await brRepo.GetByIdAsync(transaction.BookingRequestId.Value);
                if (br != null)
                {
                    br.Status = BookingRequestStatus.Paid;
                    br.UpdatedAt = DateTime.UtcNow;
                    brRepo.UpdateAsync(br);
                }
            }

            var availabilityId = transaction.CoachAvailabilityId!.Value;
            var transactionId = transaction.Id;

            var room = new Domain.Entities.InterviewRoom
            {
                CandidateId = candidateId,
                CoachId = coachId,
                ScheduledTime = startTime,
                Status = InterviewRoomStatus.Scheduled,
                DurationMinutes = duration,
                CurrentAvailabilityId = availabilityId,
                TransactionId = transactionId,
                BookingRequestId = transaction.BookingRequestId
            };

            _backgroundService.Enqueue<ICreateInterviewRoom>(
                uc => uc.ExecuteAsync(room)
            );
        }

        /// <summary>
        /// Flow B/C: BookingRequest payment — mark as Paid, split availability, and create interview rooms
        /// </summary>
        private async Task HandleBookingRequestPayment(InterviewBookingTransaction transaction)
        {
            var bookingRepo = _unitOfWork.GetRepository<IBookingRequestRepository>();
            var availabilityRepo = _unitOfWork.GetRepository<ICoachAvailabilitiesRepository>();

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

            if (bookingRequest.Type == BookingRequestType.Direct)
            {
                // Flow A: Direct booking via availability slot
                await HandleAvailabilityPayment(transaction);
                return;
            }

            if (bookingRequest.Type == BookingRequestType.External)
            {
                // Flow B: Split availability for the single booking
                var durationMinutes = bookingRequest.CoachInterviewService?.DurationMinutes ?? 60;
                var startTime = bookingRequest.RequestedStartTime!.Value;
                var endTime = startTime.AddMinutes(durationMinutes);

                var currentAvailabilityId = await SplitAvailabilityForBooking(availabilityRepo, bookingRequest.CoachId, startTime, endTime);

                // Create a single interview room
                var room = new Domain.Entities.InterviewRoom
                {
                    CandidateId = bookingRequest.CandidateId,
                    CoachId = bookingRequest.CoachId,
                    ScheduledTime = bookingRequest.RequestedStartTime,
                    DurationMinutes = durationMinutes,
                    CurrentAvailabilityId = currentAvailabilityId,
                    Status = InterviewRoomStatus.Scheduled,
                    TransactionId = transaction.Id,
                    BookingRequestId = bookingRequest.Id,
                    CoachInterviewServiceId = bookingRequest.CoachInterviewServiceId,
                    AimLevel = bookingRequest.AimLevel,
                    EvaluationResults = await CreateEvaluationResultsFromInterviewService(bookingRequest.CoachInterviewServiceId),
                    IsEvaluationCompleted = false
                };
                _backgroundService.Enqueue<ICreateInterviewRoom>(uc => uc.ExecuteAsync(room));

                _logger.LogInformation(
                    "Queued interview room creation for external BookingRequest {BookingRequestId}",
                    bookingRequest.Id);
            }
            else if (bookingRequest.Type == BookingRequestType.JDInterview)
            {
                // Flow C: Split availability for each round and create rooms
                foreach (var round in bookingRequest.Rounds.OrderBy(r => r.RoundNumber))
                {
                    var roundDuration = round.CoachInterviewService?.DurationMinutes ?? 60;
                    var roundEnd = round.StartTime.AddMinutes(roundDuration);

                    var currentAvailabilityId = await SplitAvailabilityForBooking(availabilityRepo, bookingRequest.CoachId, round.StartTime, roundEnd);

                    var room = new Domain.Entities.InterviewRoom
                    {
                        CandidateId = bookingRequest.CandidateId,
                        CoachId = bookingRequest.CoachId,
                        ScheduledTime = round.StartTime,
                        DurationMinutes = roundDuration,
                        CurrentAvailabilityId = currentAvailabilityId,
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
                    "Queued {RoundCount} interview rooms for JD BookingRequest {BookingRequestId}",
                    bookingRequest.Rounds.Count, bookingRequest.Id);
            }
        }

        private async Task<List<EvaluationResult>> CreateEvaluationResultsFromInterviewService(Guid? coachInterviewServiceId)
        {
            if (coachInterviewServiceId == null)
                return [];

            var service = await _coachInterviewServiceRepository.GetByIdWithDetailsAsync(coachInterviewServiceId.Value);

            if (service == null)
                return [];

            return [.. service.InterviewType.EvaluationStructure.Select(c => new EvaluationResult
            {
                Type = c.Type,
                Question = c.Question,
                Score = 0,
                Answer = ""
            })];
        }

        /// <summary>
        /// Finds the containing availability range and splits it around the booked time,
        /// applying a 15-minute buffer after the booking.
        /// </summary>
        private async Task<Guid?> SplitAvailabilityForBooking(
            ICoachAvailabilitiesRepository availabilityRepo,
            Guid coachId,
            DateTime bookingStart,
            DateTime bookingEnd)
        {
            var containingAvailability = await availabilityRepo.FindContainingAvailabilityAsync(
                coachId, bookingStart, bookingEnd);

            if (containingAvailability == null)
            {
                _logger.LogWarning(
                    "No containing availability found for coach {CoachId} time range {Start} - {End}. " +
                    "Skipping availability reservation (booking may be outside coach hours).",
                    coachId, bookingStart, bookingEnd);
                return null;
            }

            var (before, after) = AvailabilitySplitService.Split(containingAvailability, bookingStart, bookingEnd);

            // Keep the original row as the reserved slot and link InterviewRoom.CurrentAvailabilityId to it.
            containingAvailability.StartTime = bookingStart;
            containingAvailability.EndTime = bookingEnd;
            containingAvailability.Status = CoachAvailabilityStatus.Unavailable;
            availabilityRepo.UpdateAsync(containingAvailability);

            // Insert the split ranges
            if (before != null)
                await availabilityRepo.AddAsync(before);
            if (after != null)
                await availabilityRepo.AddAsync(after);

            _logger.LogInformation(
                "Reserved availability {AvailabilityId} for booking {Start} - {End}. " +
                "Created {RangeCount} remaining available range(s).",
                containingAvailability.Id, bookingStart, bookingEnd,
                (before != null ? 1 : 0) + (after != null ? 1 : 0));

            return containingAvailability.Id;
        }
    }
}
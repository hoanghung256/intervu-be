using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Application.Services;
using Intervu.Application.Interfaces.UseCases.Notification;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Intervu.Application.UseCases.InterviewBooking
{
    public class HandldeInterviewBookingUpdate : IHandldeInterviewBookingUpdate
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IPaymentService _paymentService;
        private readonly ICoachAvailabilitiesRepository _coachAvailabilitiesRepository;
        private readonly IBackgroundService _backgroundService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<HandldeInterviewBookingUpdate> _logger;

        public HandldeInterviewBookingUpdate(
            ITransactionRepository transactionRepository,
            IPaymentService paymentService,
            ICoachAvailabilitiesRepository coachAvailabilitiesRepository,
            IBackgroundService backgroundService,
            IUnitOfWork unitOfWork,
            ILogger<HandldeInterviewBookingUpdate> logger)
        {
            _transactionRepository = transactionRepository;
            _paymentService = paymentService;
            _coachAvailabilitiesRepository = coachAvailabilitiesRepository;
            _backgroundService = backgroundService;
            _unitOfWork = unitOfWork;
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

                transaction.Status = TransactionStatus.Paid;

                // --- Flow B/C: BookingRequest payment ---
                if (transaction.BookingRequestId != null)
                {
                    await HandleBookingRequestPayment(transaction);
                }
                // --- Flow A: Normal availability booking ---
                else if (transaction.CoachAvailabilityId != null)
                {
                    await HandleAvailabilityPayment(transaction);
                }
                else
                {
                    throw new NotFoundException("Transaction has no associated booking context");
                }
                
                // Notify candidate — booking confirmed
                _backgroundService.Enqueue<INotificationUseCase>(
                    uc => uc.CreateAsync(
                        transaction.UserId,
                        NotificationType.PaymentSuccess,
                        "Booking confirmed",
                        "Your interview has been booked successfully.",
                        "/interview?tab=upcoming",
                        null));

                // Notify coach — new booking
                _backgroundService.Enqueue<INotificationUseCase>(
                    uc => uc.CreateAsync(
                        availability.CoachId,
                        NotificationType.BookingNew,
                        "New interview booking",
                        "A candidate has booked an interview with you.",
                        "/interview?tab=upcoming",
                        null));

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                //_logger.LogError(ex, "Error while handle update booing transaction");
                throw;
            }
        }

        /// <summary>
        /// Flow A: Normal booking via coach availability slot.
        /// The availability was already split at checkout creation time.
        /// Here we just create the interview room using stored metadata.
        /// </summary>
        private Task HandleAvailabilityPayment(InterviewBookingTransaction transaction)
        {
            var candidateId = transaction.UserId;
            var coachId = transaction.CoachId
                ?? throw new NotFoundException("Transaction is missing CoachId metadata");
            var startTime = transaction.BookedStartTime
                ?? throw new NotFoundException("Transaction is missing BookedStartTime metadata");
            var duration = transaction.BookedDurationMinutes
                ?? throw new NotFoundException("Transaction is missing BookedDurationMinutes metadata");

            _backgroundService.Enqueue<ICreateInterviewRoom>(
                uc => uc.ExecuteAsync(
                    candidateId,
                    coachId,
                    transaction.CoachAvailabilityId!.Value,
                    startTime,
                    transaction.Id,
                    duration)
            );

            return Task.CompletedTask;
        }

        /// <summary>
        /// Flow B/C: BookingRequest payment — mark as Paid, split availability, and create interview rooms
        /// </summary>
        private async Task HandleBookingRequestPayment(InterviewBookingTransaction transaction)
        {
            var bookingRepo = _unitOfWork.GetRepository<IBookingRequestRepository>();
            var roomRepo = _unitOfWork.GetRepository<IInterviewRoomRepository>();
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

            if (bookingRequest.Type == BookingRequestType.External)
            {
                // Flow B: Split availability for the single booking
                var durationMinutes = bookingRequest.CoachInterviewService?.DurationMinutes ?? 60;
                var startTime = bookingRequest.RequestedStartTime!.Value;
                var endTime = startTime.AddMinutes(durationMinutes);

                await SplitAvailabilityForBooking(availabilityRepo, bookingRequest.CoachId, startTime, endTime);

                // Create a single interview room
                var room = new Domain.Entities.InterviewRoom
                {
                    CandidateId = bookingRequest.CandidateId,
                    CoachId = bookingRequest.CoachId,
                    ScheduledTime = bookingRequest.RequestedStartTime,
                    DurationMinutes = durationMinutes,
                    Status = InterviewRoomStatus.Scheduled,
                    TransactionId = transaction.Id,
                    BookingRequestId = bookingRequest.Id,
                    CoachInterviewServiceId = bookingRequest.CoachInterviewServiceId,
                    AimLevel = bookingRequest.AimLevel,
                };
                await roomRepo.AddAsync(room);

                _logger.LogInformation(
                    "Created interview room for external BookingRequest {BookingRequestId}",
                    bookingRequest.Id);
            }
            else if (bookingRequest.Type == BookingRequestType.JDInterview)
            {
                // Flow C: Split availability for each round and create rooms
                foreach (var round in bookingRequest.Rounds.OrderBy(r => r.RoundNumber))
                {
                    var roundDuration = round.CoachInterviewService?.DurationMinutes ?? 60;
                    var roundEnd = round.StartTime.AddMinutes(roundDuration);

                    await SplitAvailabilityForBooking(availabilityRepo, bookingRequest.CoachId, round.StartTime, roundEnd);

                    var room = new Domain.Entities.InterviewRoom
                    {
                        CandidateId = bookingRequest.CandidateId,
                        CoachId = bookingRequest.CoachId,
                        ScheduledTime = round.StartTime,
                        DurationMinutes = roundDuration,
                        Status = InterviewRoomStatus.Scheduled,
                        TransactionId = transaction.Id,
                        BookingRequestId = bookingRequest.Id,
                        CoachInterviewServiceId = round.CoachInterviewServiceId,
                        AimLevel = bookingRequest.AimLevel,
                        RoundNumber = round.RoundNumber,
                    };
                    await roomRepo.AddAsync(room);
                }

                _logger.LogInformation(
                    "Created {RoundCount} interview rooms for JD BookingRequest {BookingRequestId}",
                    bookingRequest.Rounds.Count, bookingRequest.Id);
            }
        }

        /// <summary>
        /// Finds the containing availability range and splits it around the booked time,
        /// applying a 15-minute buffer after the booking.
        /// </summary>
        private async Task SplitAvailabilityForBooking(
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
                    "Skipping availability split (booking may be outside coach hours).",
                    coachId, bookingStart, bookingEnd);
                return;
            }

            var (before, after) = AvailabilitySplitService.Split(containingAvailability, bookingStart, bookingEnd);

            // Remove the original availability
            availabilityRepo.DeleteAsync(containingAvailability);

            // Insert the split ranges
            if (before != null)
                await availabilityRepo.AddAsync(before);
            if (after != null)
                await availabilityRepo.AddAsync(after);

            _logger.LogInformation(
                "Split availability {AvailabilityId} for booking {Start} - {End}. " +
                "Created {RangeCount} new range(s).",
                containingAvailability.Id, bookingStart, bookingEnd,
                (before != null ? 1 : 0) + (after != null ? 1 : 0));
        }
    }
}
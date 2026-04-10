using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Application.Interfaces.UseCases.Notification;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Intervu.Application.UseCases.InterviewBooking
{
    public class ProcessBookingPayment : IProcessBookingPayment
    {
        private readonly IBackgroundService _backgroundService;
        private readonly Intervu.Application.Interfaces.UseCases.BookingRequest.ICreateEvaluationResultsUseCase _createEvaluationResults;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProcessBookingPayment> _logger;

        public ProcessBookingPayment(
            IBackgroundService backgroundService,
            Intervu.Application.Interfaces.UseCases.BookingRequest.ICreateEvaluationResultsUseCase createEvaluationResults,
            IUnitOfWork unitOfWork,
            ILogger<ProcessBookingPayment> logger)
        {
            _backgroundService = backgroundService;
            _createEvaluationResults = createEvaluationResults;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task ExecuteAsync(InterviewBookingTransaction transaction)
        {
            var bookingRepo = _unitOfWork.GetRepository<IBookingRequestRepository>();

            var bookingRequest = await bookingRepo.GetByIdWithDetailsAsync(transaction.BookingRequestId!.Value)
                ?? throw new Exceptions.NotFoundException("Booking request not found");

            if (bookingRequest.Status != BookingRequestStatus.Pending)
            {
                _logger.LogWarning(
                    "BookingRequest {Id} is not in Pending status (current: {Status}), skipping payment handling",
                    bookingRequest.Id, bookingRequest.Status);
                return;
            }

            bookingRequest.Status = BookingRequestStatus.Accepted;
            bookingRequest.ExpiresAt = DateTime.UtcNow.AddHours(48);
            bookingRequest.UpdatedAt = DateTime.UtcNow;
            bookingRepo.UpdateAsync(bookingRequest);

            foreach (var round in bookingRequest.Rounds)
            {
                var availabilityId = round.AvailabilityBlocks.FirstOrDefault()?.Id ?? Guid.Empty;
                var duration = (int)(round.EndTime - round.StartTime).TotalMinutes;

                if (bookingRequest.Status == BookingRequestStatus.Accepted)
                {
                    var room = new Domain.Entities.InterviewRoom
                    {
                        CandidateId = bookingRequest.CandidateId,
                        CoachId = bookingRequest.CoachId,
                        ScheduledTime = round.StartTime,
                        DurationMinutes = duration,
                        CurrentAvailabilityId = availabilityId,
                        Status = InterviewRoomStatus.Scheduled,
                        TransactionId = transaction.Id,
                        BookingRequestId = bookingRequest.Id,
                        CoachInterviewServiceId = round.CoachInterviewServiceId,
                        AimLevel = bookingRequest.AimLevel,
                        RoundNumber = round.RoundNumber,
                        EvaluationResults = await _createEvaluationResults.ExecuteAsync(round.CoachInterviewServiceId),
                        IsEvaluationCompleted = false
                    };

                    _backgroundService.Enqueue<ICreateInterviewRoom>(uc => uc.ExecuteAsync(room));
                }
            }

            var availabilityRepo = _unitOfWork.GetRepository<ICoachAvailabilitiesRepository>();
            foreach (var round in bookingRequest.Rounds)
            {
                if (round.AvailabilityBlocks == null) continue;
                foreach (var block in round.AvailabilityBlocks)
                {
                    if (block.Status == CoachAvailabilityStatus.Reserved)
                    {
                        block.Status = CoachAvailabilityStatus.Booked;
                        availabilityRepo.UpdateAsync(block);
                    }
                }
            }

            SentNotification(
                bookingRequest.Status == BookingRequestStatus.Accepted,
                bookingRequest.CandidateId,
                bookingRequest.CoachId);

            _logger.LogInformation(
                "BookingRequest {BookingRequestId} marked as Paid, availability blocks confirmed as Booked",
                bookingRequest.Id);
        }

        private void SentNotification(bool isBookingRequestAccepted, Guid candidateId, Guid coachId)
        {
            var candidateTitle = isBookingRequestAccepted ? "Booking confirmed" : "Payment received";
            var candidateMessage = isBookingRequestAccepted
                ? "Your payment was successful. Your booking has been confirmed."
                : "Your payment was successful. The booking is awaiting coach approval.";

            _backgroundService.Enqueue<INotificationUseCase>(
                uc => uc.CreateAsync(
                    candidateId,
                    NotificationType.BookingNew,
                    candidateTitle,
                    candidateMessage,
                    "/booking?tab=pending",
                    null));

            var coachMessage = isBookingRequestAccepted
                ? "A candidate has paid for an interview and the booking is now confirmed."
                : "A candidate has paid for an interview. Please review and approve or reject.";

            _backgroundService.Enqueue<INotificationUseCase>(
                uc => uc.CreateAsync(
                    coachId,
                    NotificationType.BookingNew,
                    "New booking request",
                    coachMessage,
                    "/booking?tab=pending",
                    null));
        }
    }
}

using Intervu.Application.Exceptions;
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
    public class HandldeInterviewBookingUpdate : IHandldeInterviewBookingUpdate
    {
        private readonly IPaymentService _paymentService;
        private readonly IBackgroundService _backgroundService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<HandldeInterviewBookingUpdate> _logger;

        public HandldeInterviewBookingUpdate(
            IPaymentService paymentService,
            IBackgroundService backgroundService,
            IUnitOfWork unitOfWork,
            ILogger<HandldeInterviewBookingUpdate> logger)
        {
            _paymentService = paymentService;
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

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        /// <summary>
        /// Unified payment handler for all booking request types (Direct, External, JD).
        /// Marks booking as Paid, transitions reserved availability blocks to Booked,
        /// and resets expiry for the 48h coach response window.
        /// </summary>
        private async Task HandleBookingRequestPayment(InterviewBookingTransaction transaction)
        {
            var bookingRepo = _unitOfWork.GetRepository<IBookingRequestRepository>();

            var bookingRequest = await bookingRepo.GetByIdWithDetailsAsync(transaction.BookingRequestId!.Value)
                ?? throw new NotFoundException("Booking request not found");

            // Accept only Pending status — payment must follow booking creation
            if (bookingRequest.Status != BookingRequestStatus.Pending)
            {
                _logger.LogWarning(
                    "BookingRequest {Id} is not in Pending status (current: {Status}), skipping payment handling",
                    bookingRequest.Id, bookingRequest.Status);
                return;
            }

            // Transition to PendingForApprovalAfterPayment — reset expiry for the 48h coach response window
            // bookingRequest.Status = BookingRequestStatus.PendingForApprovalAfterPayment;
            bookingRequest.Status = BookingRequestStatus.Accepted; // Auto-accept for zero-price bookings
            bookingRequest.ExpiresAt = DateTime.UtcNow.AddHours(48);
            bookingRequest.UpdatedAt = DateTime.UtcNow;
            bookingRepo.UpdateAsync(bookingRequest);

            foreach (var round in bookingRequest.Rounds)
            {
                var availabilityId = round.AvailabilityBlocks.FirstOrDefault()?.Id ?? Guid.Empty;
                var duration = (int)(round.EndTime - round.StartTime).TotalMinutes;

                // Create room(s) only for accepted bookings
                if (bookingRequest.Status == BookingRequestStatus.Accepted)
                {
                    _backgroundService.Enqueue<ICreateInterviewRoom>(
                        uc => uc.ExecuteAsync(
                            bookingRequest.CandidateId,
                            bookingRequest.CoachId,
                            availabilityId,
                            round.StartTime,
                            transaction.Id,
                            duration,
                            bookingRequest.Id
                    ));
                }
            }

            // Upgrade all reserved availability blocks to Booked now that payment is confirmed
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

            // Sent notification
            SentNotification(
                bookingRequest.Status == BookingRequestStatus.Accepted,
                bookingRequest.CandidateId,
                bookingRequest.CoachId
            );

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

            // Notify candidate — payment received, awaiting coach approval
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

            // Notify coach — new paid booking waiting for review
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

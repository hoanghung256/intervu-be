using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;
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

                var candidateId = transaction.UserId;

                // Notify candidate — payment received, awaiting coach approval
                _backgroundService.Enqueue<INotificationUseCase>(
                    uc => uc.CreateAsync(
                        candidateId,
                        NotificationType.BookingNew,
                        "Payment received",
                        "Your payment was successful. The booking is awaiting coach approval.",
                        "/booking?tab=pending",
                        null));

                var bookingRepo = _unitOfWork.GetRepository<IBookingRequestRepository>();
                var bookingRequest = await bookingRepo.GetByIdAsync(transaction.BookingRequestId.Value);
                if (bookingRequest != null)
                {
                    var coachId = bookingRequest.CoachId;
                    // Notify coach — new paid booking waiting for review
                    _backgroundService.Enqueue<INotificationUseCase>(
                        uc => uc.CreateAsync(
                            coachId,
                            NotificationType.BookingNew,
                            "New booking request",
                            "A candidate has paid for an interview. Please review and approve or reject.",
                            "/booking?tab=pending",
                            null));
                }

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
            bookingRequest.Status = BookingRequestStatus.PendingForApprovalAfterPayment;
            bookingRequest.ExpiresAt = DateTime.UtcNow.AddHours(48);
            bookingRequest.UpdatedAt = DateTime.UtcNow;
            bookingRepo.UpdateAsync(bookingRequest);

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

            _logger.LogInformation(
                "BookingRequest {BookingRequestId} marked as Paid, availability blocks confirmed as Booked",
                bookingRequest.Id);
        }

    }
}

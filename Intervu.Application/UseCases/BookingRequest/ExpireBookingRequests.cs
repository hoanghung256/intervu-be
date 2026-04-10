using Intervu.Application.Interfaces.UseCases.BookingRequest;
using Intervu.Application.Utils;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Intervu.Application.Interfaces.ExternalServices.Email;

namespace Intervu.Application.UseCases.BookingRequest
{
    internal class ExpireBookingRequests : IExpireBookingRequests
    {
        private readonly IBookingRequestRepository _bookingRepo;
        private readonly ITransactionRepository _transactionRepo;
        private readonly ICoachAvailabilitiesRepository _availabilityRepo;
        private readonly IEmailService _emailService;

        public ExpireBookingRequests(
            IBookingRequestRepository bookingRepo,
            ITransactionRepository transactionRepo,
            ICoachAvailabilitiesRepository availabilityRepo,
            IEmailService emailService)
        {
            _bookingRepo = bookingRepo;
            _transactionRepo = transactionRepo;
            _availabilityRepo = availabilityRepo;
            _emailService = emailService;
        }

        public async Task<int> ExecuteAsync()
        {
            int count = 0;

            // Pending bookings that expired without payment — mark as Expired and free reserved blocks
            var expiredPending = (await _bookingRepo.GetExpiredPendingRequestsAsync()).ToList();
            foreach (var request in expiredPending)
            {
                request.Status = BookingRequestStatus.Expired;
                request.UpdatedAt = DateTime.UtcNow;
                _bookingRepo.UpdateAsync(request);
                FreeAvailabilityBlocks(request);
                count++;
            }

            // Paid bookings where coach did not respond in time — expire, refund, notify
            var expiredPaid = (await _bookingRepo.GetExpiredPaidRequestsAsync()).ToList();
            foreach (var request in expiredPaid)
            {
                request.Status = BookingRequestStatus.Expired;
                request.UpdatedAt = DateTime.UtcNow;
                _bookingRepo.UpdateAsync(request);

                await ProcessRefundAsync(request);
                FreeAvailabilityBlocks(request);
                await SendExpiryNotificationEmailsAsync(request);

                count++;
            }

            if (count > 0)
                await _bookingRepo.SaveChangesAsync();

            return count;
        }

        private async Task ProcessRefundAsync(Domain.Entities.BookingRequest request)
        {
            // Cancel payout — coach will not be paid
            var payout = await _transactionRepo.GetByBookingRequestId(request.Id, TransactionType.Payout);
            if (payout != null)
            {
                payout.Status = TransactionStatus.Cancel;
                _transactionRepo.UpdateAsync(payout);
            }

            // Issue 100% refund to candidate
            var payment = await _transactionRepo.GetByBookingRequestId(request.Id, TransactionType.Payment);
            if (payment != null && payment.Amount > 0)
            {
                await _transactionRepo.AddAsync(new InterviewBookingTransaction
                {
                    OrderCode = RandomGenerator.GenerateOrderCode(),
                    UserId = request.CandidateId,
                    BookingRequestId = request.Id,
                    Amount = payment.Amount,
                    Type = TransactionType.Refund,
                    Status = TransactionStatus.Created
                });
            }
        }

        private void FreeAvailabilityBlocks(Domain.Entities.BookingRequest request)
        {
            foreach (var round in request.Rounds)
            {
                if (round.AvailabilityBlocks == null) continue;
                foreach (var block in round.AvailabilityBlocks)
                {
                    block.Status = CoachAvailabilityStatus.Available;
                    block.InterviewRoundId = null;
                    _availabilityRepo.UpdateAsync(block);
                }
            }
        }

        private async Task SendExpiryNotificationEmailsAsync(Domain.Entities.BookingRequest request)
        {
            var candidateEmail = request.Candidate?.User?.Email;
            var coachEmail = request.Coach?.User?.Email;

            var bookingId = request.Id.ToString()[..8].ToUpper();

            if (!string.IsNullOrEmpty(candidateEmail))
            {
                await _emailService.SendEmailAsync(
                    candidateEmail,
                    "Your booking request has expired",
                    $"<p>Your booking request <strong>{bookingId}</strong> has expired because the coach did not respond within 48 hours.</p>" +
                    $"<p>A full refund will be processed to your account.</p>");
            }

            if (!string.IsNullOrEmpty(coachEmail))
            {
                await _emailService.SendEmailAsync(
                    coachEmail,
                    "A booking request has expired",
                    $"<p>Booking request <strong>{bookingId}</strong> has expired because you did not respond within 48 hours.</p>" +
                    $"<p>The candidate will receive a full refund.</p>");
            }
        }
    }
}

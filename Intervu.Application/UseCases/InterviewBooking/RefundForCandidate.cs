using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;
using Intervu.Application.Interfaces.UseCases.Notification;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Intervu.Application.UseCases.InterviewBooking
{
    public class RefundForCandidate : IRefundForCandidate
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICandidateProfileRepository _candidateProfileRepository;
        private readonly IPaymentService _paymentService;
        private readonly IBackgroundService _jobService;
        private readonly ILogger<RefundForCandidate> _logger;
        private readonly IBankFieldProtector _bankFieldProtector;

        public RefundForCandidate(
            ITransactionRepository transactionRepository,
            ICandidateProfileRepository candidateProfileRepository,
            IPaymentService paymentService,
            IBackgroundService jobService,
            ILogger<RefundForCandidate> logger,
            IBankFieldProtector bankFieldProtector)
        {
            _transactionRepository = transactionRepository;
            _candidateProfileRepository = candidateProfileRepository;
            _paymentService = paymentService;
            _jobService = jobService;
            _logger = logger;
            _bankFieldProtector = bankFieldProtector;
        }

        public async Task ExecuteAsync(Guid bookingRequestId)
        {
            _logger.LogInformation("Starting RefundForCandidate for BookingRequestId: {BookingRequestId}", bookingRequestId);

            _logger.LogInformation("Finding refund transaction for BookingRequestId: {BookingRequestId}", bookingRequestId);

            // Find payout/refund transaction via BookingRequest
            InterviewBookingTransaction? t = await _transactionRepository.GetByBookingRequestId(bookingRequestId, TransactionType.Refund);

            if (t == null)
            {
                _logger.LogWarning("No refund transaction found for BookingRequestId: {BookingRequestId}", bookingRequestId);
                return;
            }

            var candidateId = t.UserId;
            var candidate = await _candidateProfileRepository.GetProfileByIdAsync(candidateId)
                ?? throw new Exception($"Candidate profile not found for CandidateId: {candidateId}");

            _logger.LogInformation("Found transaction {TransactionId} with Status: {Status}, Amount: {Amount}", t.Id, t.Status, t.Amount);

            if (t.Status == TransactionStatus.Created)
            {
                // Skip payouts with non-positive amount
                if (t.Amount <= 0)
                {
                    _logger.LogWarning("Transaction {TransactionId} amount is {Amount}, skipping payout.", t.Id, t.Amount);
                    return;
                }

                _logger.LogInformation("Processing payout to BankBin: {BankBin}, Account: {Account}", candidate.BankBinNumber, candidate.BankAccountNumberMasked);

                var plainAccount = _bankFieldProtector.Decrypt(candidate.BankAccountNumber);
                _logger.LogInformation("Decrypted bank account: {bankAccount}", plainAccount);
                await _paymentService.CreateSpendOrderAsync(
                    t.Amount,
                    $"REFUND",
                    candidate.BankBinNumber,
                    plainAccount
                );
                
                _logger.LogInformation("Refund successfully sent to PaymentService for transaction: {TransactionId}", t.Id);
            }
            else
            {
                _logger.LogWarning("Transaction {TransactionId} is not in Created status. Current: {Status}. Skipping refund.", t.Id, t.Status);
            }
            
            var amount = t.Amount;
            _logger.LogInformation("Queueing notification for Candidate: {CandidateId} for amount: {Amount}", candidateId, amount);
            
            _jobService.Enqueue<INotificationUseCase>(uc => uc.CreateAsync(
                candidateId,
                NotificationType.PaymentSuccess,
                "Refund Processed",
                $"Your refund of {amount:N0} resources has been processed.",
                "/dashboard/wallet",
                null
            ));
            
            _logger.LogInformation("Finished RefundForCandidate for BookingRequestId: {BookingRequestId}", bookingRequestId);
        }
    }
}

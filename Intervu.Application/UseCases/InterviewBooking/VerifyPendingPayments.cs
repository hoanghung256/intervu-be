using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Intervu.Application.UseCases.InterviewBooking
{
    public class VerifyPendingPayments : IVerifyPendingPayments
    {
        private readonly ITransactionRepository _transactionRepo;
        private readonly IPaymentService _paymentService;
        private readonly IProcessBookingPayment _processBookingPayment;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<VerifyPendingPayments> _logger;

        public VerifyPendingPayments(
            ITransactionRepository transactionRepo,
            IPaymentService paymentService,
            IProcessBookingPayment processBookingPayment,
            IUnitOfWork unitOfWork,
            ILogger<VerifyPendingPayments> logger)
        {
            _transactionRepo = transactionRepo;
            _paymentService = paymentService;
            _processBookingPayment = processBookingPayment;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<int> ExecuteAsync()
        {
            // Fetch snapshot with AsNoTracking — each confirmed transaction is processed
            // inside its own UnitOfWork transaction scope below.
            var pendingTransactions = await _transactionRepo.GetAllCreatedPaymentsAsync();

            if (pendingTransactions.Count == 0)
                return 0;

            _logger.LogInformation(
                "PaymentVerificationJob: found {Count} pending payment transaction(s) to verify",
                pendingTransactions.Count);

            int confirmedCount = 0;

            foreach (var snapshot in pendingTransactions)
            {
                bool isPaid;
                try
                {
                    isPaid = await _paymentService.IsPaymentPaidAsync(snapshot.OrderCode);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "PaymentVerificationJob: PayOS query failed for orderCode {OrderCode}, will retry next cycle",
                        snapshot.OrderCode);
                    continue;
                }

                if (!isPaid)
                    continue;

                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    var transactionRepo = _unitOfWork.GetRepository<ITransactionRepository>();
                    var transaction = await transactionRepo.Get(snapshot.OrderCode, TransactionType.Payment);

                    if (transaction == null)
                    {
                        _logger.LogWarning(
                            "PaymentVerificationJob: transaction for orderCode {OrderCode} not found during processing",
                            snapshot.OrderCode);
                        await _unitOfWork.RollbackTransactionAsync();
                        continue;
                    }

                    // Idempotency guard — webhook may have processed this between our snapshot and now
                    if (transaction.Status == TransactionStatus.Paid)
                    {
                        _logger.LogDebug(
                            "PaymentVerificationJob: orderCode {OrderCode} already processed by webhook, skipping",
                            snapshot.OrderCode);
                        await _unitOfWork.RollbackTransactionAsync();
                        continue;
                    }

                    if (transaction.BookingRequestId == null)
                    {
                        _logger.LogWarning(
                            "PaymentVerificationJob: orderCode {OrderCode} has no BookingRequestId, skipping",
                            snapshot.OrderCode);
                        await _unitOfWork.RollbackTransactionAsync();
                        continue;
                    }

                    transaction.Status = TransactionStatus.Paid;
                    await _processBookingPayment.ExecuteAsync(transaction);

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    confirmedCount++;

                    _logger.LogInformation(
                        "PaymentVerificationJob: confirmed payment for orderCode {OrderCode}",
                        snapshot.OrderCode);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError(ex,
                        "PaymentVerificationJob: failed to process confirmed payment for orderCode {OrderCode}",
                        snapshot.OrderCode);
                }
            }

            return confirmedCount;
        }
    }
}

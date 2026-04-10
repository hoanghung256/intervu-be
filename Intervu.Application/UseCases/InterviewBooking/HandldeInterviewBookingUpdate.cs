using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Intervu.Application.UseCases.InterviewBooking
{
    public class HandldeInterviewBookingUpdate : IHandldeInterviewBookingUpdate
    {
        private readonly IPaymentService _paymentService;
        private readonly IProcessBookingPayment _processBookingPayment;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<HandldeInterviewBookingUpdate> _logger;

        public HandldeInterviewBookingUpdate(
            IPaymentService paymentService,
            IProcessBookingPayment processBookingPayment,
            IUnitOfWork unitOfWork,
            ILogger<HandldeInterviewBookingUpdate> logger)
        {
            _paymentService = paymentService;
            _processBookingPayment = processBookingPayment;
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

                var transaction = await transactionRepo.Get(orderCode, TransactionType.Payment)
                    ?? throw new NotFoundException("Booking transaction not found");

                if (transaction.Status == TransactionStatus.Paid)
                {
                    _logger.LogInformation(
                        "Payment webhook already handled for booking transaction {TransactionId}",
                        transaction.Id);
                    return;
                }

                if (transaction.BookingRequestId == null)
                    throw new NotFoundException("Transaction has no associated booking request");

                transaction.Status = TransactionStatus.Paid;

                await _processBookingPayment.ExecuteAsync(transaction);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}

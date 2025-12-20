using Intervu.Application.Interfaces.UseCases.Transaction;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.Transaction
{
    public class UpdateTransactionStatus : IUpdateTransactionStatus
    {
        private readonly ITransactionRepository _transactionRepository;

        public UpdateTransactionStatus(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        public async Task<bool> ExecuteAsync(int payosOrderCode, TransactionStatus status)
        {
            Domain.Entities.InterviewBookingTransaction? t = await _transactionRepository.GetByAvailabilityId(payosOrderCode);

            if (t == null) return false;

            t.Status = status;
            await _transactionRepository.SaveChangesAsync();

            return true;
        }
    }
}

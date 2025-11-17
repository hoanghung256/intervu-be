using Intervu.Application.Interfaces.Repositories;
using Intervu.Application.Interfaces.UseCases.Transaction;
using Intervu.Domain.Entities.Constants;

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
            Domain.Entities.Transaction? t = await _transactionRepository.GetByPayOSOrderCode(payosOrderCode);

            if (t == null) return false;

            t.Status = status;
            await _transactionRepository.SaveChangesAsync();

            return true;
        }
    }
}

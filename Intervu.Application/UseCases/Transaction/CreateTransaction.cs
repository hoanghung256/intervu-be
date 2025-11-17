using Intervu.Application.Interfaces.Repositories;
using Intervu.Application.Interfaces.UseCases.Transaction;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.UseCases.Transaction
{
    public class CreateTransaction : ICreateTransaction
    {
        private readonly ITransactionRepository _transactionRepository;

        public CreateTransaction(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        public async Task ExecuteAsync(int userId, int payosOrderCode, int amount, TransactionType type)
        {
            await _transactionRepository.AddAsync(new Domain.Entities.Transaction
            {
                UserId = userId,
                PayOSOrderCode = payosOrderCode,
                Amount = amount,
                Type = type,
                Status = TransactionStatus.Created
            });
        }
    }
}

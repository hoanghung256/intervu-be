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

        public async Task ExecuteAsync(int userId, int avaiabilityId, int amount, TransactionType type)
        {
            await _transactionRepository.AddAsync(new Domain.Entities.InterviewBookingTransaction
            {
                UserId = userId,
                InterviewerAvailabilityId = avaiabilityId,
                Amount = amount,
                Type = type,
                Status = TransactionStatus.Created
            });
        }
    }
}

using Intervu.Application.Interfaces.UseCases.Transaction;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.Transaction
{
    public class CreateTransaction : ICreateTransaction
    {
        private readonly ITransactionRepository _transactionRepository;

        public CreateTransaction(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        public async Task ExecuteAsync(Guid userId, Guid avaiabilityId, int amount, TransactionType type)
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

using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.Interfaces.UseCases.Transaction
{
    public interface ICreateTransaction
    {
        Task ExecuteAsync(Guid userId, Guid avaiability, int amount, TransactionType type);
    }
}

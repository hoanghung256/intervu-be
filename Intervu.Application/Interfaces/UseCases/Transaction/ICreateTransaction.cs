using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.Interfaces.UseCases.Transaction
{
    public interface ICreateTransaction
    {
        Task ExecuteAsync(int userId, int avaiability, int amount, TransactionType type);
    }
}

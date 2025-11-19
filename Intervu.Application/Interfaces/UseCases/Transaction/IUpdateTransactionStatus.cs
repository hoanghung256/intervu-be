using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.Interfaces.UseCases.Transaction
{
    public interface IUpdateTransactionStatus
    {
        Task<bool> ExecuteAsync(int payosOrderCode, TransactionStatus status);
    }
}

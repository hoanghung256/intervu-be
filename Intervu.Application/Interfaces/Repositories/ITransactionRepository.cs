using Intervu.Domain.Entities;

namespace Intervu.Application.Interfaces.Repositories
{
    public interface ITransactionRepository : IRepositoryBase<Transaction>
    {
        Task<Transaction?> GetByPayOSOrderCode(int payosOrderCode);
    }
}

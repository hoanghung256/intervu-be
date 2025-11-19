using Intervu.Application.Common;
using Intervu.Domain.Entities;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.Repositories
{
    public interface IPaymentRepository : IRepositoryBase<Payment>
    {
        Task<PagedResult<Payment>> GetPagedPaymentsAsync(int page, int pageSize);
        Task<decimal> GetTotalRevenueAsync();
        Task<int> GetTotalPaymentsCountAsync();
    }
}

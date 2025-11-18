using Intervu.Application.Common;
using Intervu.Application.DTOs.Admin;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Admin
{
    public interface IGetAllPayments
    {
        Task<PagedResult<PaymentDto>> ExecuteAsync(int page, int pageSize);
    }
}

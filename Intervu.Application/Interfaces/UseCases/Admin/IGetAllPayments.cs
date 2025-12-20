using Intervu.Application.DTOs.Admin;
using Intervu.Application.DTOs.Common;

namespace Intervu.Application.Interfaces.UseCases.Admin
{
    public interface IGetAllPayments
    {
        Task<PagedResult<PaymentDto>> ExecuteAsync(int page, int pageSize);
    }
}

using Intervu.Application.DTOs.Admin;
using Intervu.Application.DTOs.Common;
using Intervu.Domain.Entities.Constants;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Admin
{
    public interface IGetAllTransactionsForAdmin
    {
        Task<PagedResult<AdminTransactionDto>> ExecuteAsync(
            int page,
            int pageSize,
            TransactionType? type = null,
            TransactionStatus? status = null);
    }
}

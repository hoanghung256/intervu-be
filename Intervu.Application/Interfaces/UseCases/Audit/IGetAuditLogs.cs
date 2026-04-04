using Intervu.Application.DTOs.Common;
using Intervu.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Audit
{
    public interface IGetAuditLogs
    {
        Task<IEnumerable<AuditLog>> ExecuteAsync();
        Task<PagedResult<AuditLog>> ExecutePagedAsync(int pageNumber, int pageSize);
    }
}

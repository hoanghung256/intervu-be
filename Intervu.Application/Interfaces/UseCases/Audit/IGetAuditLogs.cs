using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.Audit;
using Intervu.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Audit
{
    public interface IGetAuditLogs
    {
        Task<IEnumerable<AuditLog>> ExecuteAsync();
        Task<PagedResult<AuditLog>> ExecutePagedAsync(int pageNumber, int pageSize);
        Task<PagedResult<AuditLogItemDto>> ExecuteByRoomAsync(Guid roomId, int pageNumber, int pageSize);
    }
}

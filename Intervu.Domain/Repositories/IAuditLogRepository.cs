using Intervu.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intervu.Domain.Repositories
{
    public interface IAuditLogRepository : IRepositoryBase<AuditLog>
    {
        Task<IEnumerable<AuditLog>> GetAllAsync();
        Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);
        Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetPagedByRoomIdAsync(Guid roomId, int pageNumber, int pageSize);
    }
}

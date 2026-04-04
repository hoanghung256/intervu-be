using Intervu.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intervu.Domain.Repositories
{
    public interface IAuditLogRepository : IRepositoryBase<AuditLog>
    {
        Task<IEnumerable<AuditLog>> GetAllAsync();
    }
}

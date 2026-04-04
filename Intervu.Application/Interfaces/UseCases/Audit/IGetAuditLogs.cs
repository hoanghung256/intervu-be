using Intervu.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Audit
{
    public interface IGetAuditLogs
    {
        Task<IEnumerable<AuditLog>> ExecuteAsync();
    }
}

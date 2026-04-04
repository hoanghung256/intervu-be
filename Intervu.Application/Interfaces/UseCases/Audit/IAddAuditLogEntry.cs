using Intervu.Domain.Entities;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Audit
{
    public interface IAddAuditLogEntry
    {
        Task ExecuteAsync(AuditLog entry);
    }
}

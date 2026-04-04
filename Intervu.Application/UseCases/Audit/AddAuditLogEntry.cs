using Intervu.Application.Interfaces.UseCases.Audit;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Audit
{
    public class AddAuditLogEntry : IAddAuditLogEntry
    {
        private readonly IAuditLogRepository _auditLogRepository;

        public AddAuditLogEntry(IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }

        public async Task ExecuteAsync(AuditLog entry)
        {
            await _auditLogRepository.AddAsync(entry);
            await _auditLogRepository.SaveChangesAsync();
        }
    }
}

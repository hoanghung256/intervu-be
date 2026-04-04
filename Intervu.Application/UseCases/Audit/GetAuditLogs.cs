using Intervu.Application.Interfaces.UseCases.Audit;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Audit
{
    public class GetAuditLogs : IGetAuditLogs
    {
        private readonly IAuditLogRepository _auditLogRepository;

        public GetAuditLogs(IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }

        public async Task<IEnumerable<AuditLog>> ExecuteAsync()
        {
            return await _auditLogRepository.GetAllAsync();
        }
    }
}

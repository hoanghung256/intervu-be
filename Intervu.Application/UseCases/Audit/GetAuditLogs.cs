using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.Audit;
using Intervu.Application.Interfaces.UseCases.Audit;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Audit
{
    public class GetAuditLogs : IGetAuditLogs
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IUserRepository _userRepository;

        public GetAuditLogs(IAuditLogRepository auditLogRepository, IUserRepository userRepository)
        {
            _auditLogRepository = auditLogRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<AuditLog>> ExecuteAsync()
        {
            return await _auditLogRepository.GetAllAsync();
        }

        public async Task<PagedResult<AuditLog>> ExecutePagedAsync(int pageNumber, int pageSize)
        {
            var (items, totalCount) = await _auditLogRepository.GetPagedAsync(pageNumber, pageSize);
            return new PagedResult<AuditLog>(items.ToList(), totalCount, pageSize, pageNumber);
        }

        public async Task<PagedResult<AuditLogItemDto>> ExecuteByRoomAsync(Guid roomId, int pageNumber, int pageSize)
        {
            var (items, totalCount) = await _auditLogRepository.GetPagedByRoomIdAsync(roomId, pageNumber, pageSize);

            var logs = items.ToList();
            var userIds = logs.Where(x => x.UserId.HasValue).Select(x => x.UserId!.Value).Distinct().ToList();

            var users = new Dictionary<Guid, User>();
            foreach (var userId in userIds)
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user != null)
                {
                    users[userId] = user;
                }
            }

            var dtos = logs.Select(x => new AuditLogItemDto
            {
                Id = x.Id,
                UserId = x.UserId,
                UserName = x.UserId.HasValue && users.TryGetValue(x.UserId.Value, out var user) ? user.FullName : null,
                Message = x.Content,
                Metadata = x.MetaData,
                EventType = (int)x.EventType,
                Timestamp = x.Timestamp
            }).ToList();

            return new PagedResult<AuditLogItemDto>(dtos, totalCount, pageSize, pageNumber);
        }
    }
}

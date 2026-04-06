using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class AuditLogRepository : RepositoryBase<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(IntervuPostgreDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<AuditLog>> GetAllAsync()
        {
            return await _context.AuditLogs
                .OrderByDescending(x => x.Timestamp)
                .ToListAsync();
        }

        public async Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
        {
            var query = _context.AuditLogs.AsNoTracking();
            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(x => x.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetPagedByRoomIdAsync(Guid roomId, int pageNumber, int pageSize)
        {
            var roomIdText = roomId.ToString();
            string searchPattern = $"%{roomIdText}%";

            // Use FromSqlInterpolated to cast jsonb columns to text for ILIKE support in PostgreSQL
            var query = _context.AuditLogs
                .FromSqlInterpolated($@"
                    SELECT * FROM ""AuditLogs"" 
                    WHERE ""Content""::text ILIKE {searchPattern} 
                       OR (""MetaData"" IS NOT NULL AND ""MetaData""::text ILIKE {searchPattern})")
                .AsNoTracking();

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(x => x.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}

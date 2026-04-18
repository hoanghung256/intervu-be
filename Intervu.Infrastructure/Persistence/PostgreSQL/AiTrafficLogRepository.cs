using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class AiTrafficLogRepository : RepositoryBase<AiTrafficLog>, IAiTrafficLogRepository
    {
        public AiTrafficLogRepository(IntervuPostgreDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<AiTrafficLog>> GetByTimeframeAsync(DateTime from, DateTime to)
        {
            return await _context.AiTrafficLogs
                .AsNoTracking()
                .Where(x => x.Timestamp >= from && x.Timestamp <= to)
                .OrderByDescending(x => x.Timestamp)
                .ToListAsync();
        }

        public async Task<(IReadOnlyList<AiTrafficLog> Items, int TotalCount)> QueryAsync(
            DateTime from,
            DateTime to,
            string? provider,
            string? endpointContains,
            int page,
            int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 50;
            if (pageSize > 500) pageSize = 500;

            var query = _context.AiTrafficLogs
                .AsNoTracking()
                .Where(x => x.Timestamp >= from && x.Timestamp <= to);

            if (!string.IsNullOrWhiteSpace(provider))
            {
                var providerTrimmed = provider.Trim();
                query = query.Where(x => x.Provider.ToLower() == providerTrimmed.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(endpointContains))
            {
                var pattern = $"%{endpointContains.Trim()}%";
                query = query.Where(x => EF.Functions.ILike(x.EndpointName, pattern));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<IReadOnlyList<string>> GetDistinctProvidersAsync()
        {
            return await _context.AiTrafficLogs
                .AsNoTracking()
                .Select(x => x.Provider)
                .Where(p => p != null && p != "")
                .Distinct()
                .OrderBy(p => p)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<string>> GetDistinctEndpointsAsync()
        {
            return await _context.AiTrafficLogs
                .AsNoTracking()
                .Select(x => x.EndpointName)
                .Where(e => e != null && e != "")
                .Distinct()
                .OrderBy(e => e)
                .ToListAsync();
        }
    }
}

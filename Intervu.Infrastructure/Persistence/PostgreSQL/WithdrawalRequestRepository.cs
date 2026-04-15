using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class WithdrawalRequestRepository(IntervuPostgreDbContext context)
        : RepositoryBase<WithdrawalRequest>(context), IWithdrawalRequestRepository
    {
        public async Task<WithdrawalRequest?> GetByIdWithUserAsync(Guid id)
        {
            return await _context.WithdrawalRequests
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<(IReadOnlyList<WithdrawalRequest> Items, int TotalCount)> GetPagedAsync(
            WithdrawalStatus? status, int page, int pageSize)
        {
            var query = _context.WithdrawalRequests.AsNoTracking();

            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<(IReadOnlyList<WithdrawalRequest> Items, int TotalCount)> GetPagedByUserAsync(
            Guid userId, int page, int pageSize)
        {
            var query = _context.WithdrawalRequests
                .AsNoTracking()
                .Where(x => x.UserId == userId);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<List<WithdrawalRequest>> GetByUserIdAsync(Guid userId)
        {
            return await _context.WithdrawalRequests
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> HasPendingWithdrawalAsync(Guid userId)
        {
            return await _context.WithdrawalRequests
                .AnyAsync(x => x.UserId == userId && x.Status == WithdrawalStatus.Pending);
        }
    }
}

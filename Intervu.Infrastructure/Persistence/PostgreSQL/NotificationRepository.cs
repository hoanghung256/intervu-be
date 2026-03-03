using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;
namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class NotificationRepository(IntervuPostgreDbContext context)
        : RepositoryBase<Notification>(context), INotificationRepository
    {
        public async Task<(IReadOnlyList<Notification> Items, int TotalCount)> GetByUserIdAsync(
            Guid userId, int page, int pageSize)
        {
            var query = _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task MarkAsReadAsync(Guid notificationId)
        {
            await _context.Notifications
                .Where(n => n.Id == notificationId)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
        }

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
        }

        public async Task<bool> ExistsAsync(Guid userId, NotificationType type, Guid referenceId)
        {
            return await _context.Notifications
                .AnyAsync(n => n.UserId == userId && n.Type == type && n.ReferenceId == referenceId);
        }
    }
}

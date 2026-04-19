using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Entities.Projections;
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
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null) return;
            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            var unread = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();
            foreach (var n in unread) n.IsRead = true;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(Guid userId, NotificationType type, Guid referenceId)
        {
            return await _context.Notifications
                .AnyAsync(n => n.UserId == userId && n.Type == type && n.ReferenceId == referenceId);
        }

        public async Task AddRangeAsync(IEnumerable<Notification> notifications)
        {
            await _context.Notifications.AddRangeAsync(notifications);
        }

        public async Task<(IReadOnlyList<AdminBroadcastLogEntry> Items, int TotalCount)> GetAdminBroadcastLogsAsync(int page, int pageSize)
        {
            const int MaxScanRows = 20000;

            var rows = await _context.Notifications
                .AsNoTracking()
                .Where(n => n.ReferenceId == null)
                .OrderByDescending(n => n.CreatedAt)
                .Take(MaxScanRows)
                .Select(n => new
                {
                    n.CreatedAt,
                    n.Type,
                    n.Title,
                    n.Message,
                    n.ActionUrl,
                    Role = n.User != null ? n.User.Role : (UserRole?)null
                })
                .ToListAsync();

            var grouped = rows
                .GroupBy(n => new { n.CreatedAt, n.Type, n.Title, n.Message, n.ActionUrl })
                .Select(g => new AdminBroadcastLogEntry
                {
                    CreatedAt = g.Key.CreatedAt,
                    Type = g.Key.Type,
                    Title = g.Key.Title,
                    Message = g.Key.Message,
                    ActionUrl = g.Key.ActionUrl,
                    TotalRecipients = g.Count(),
                    CandidateRecipients = g.Count(x => x.Role == UserRole.Candidate),
                    CoachRecipients = g.Count(x => x.Role == UserRole.Coach),
                    AdminRecipients = g.Count(x => x.Role == UserRole.Admin),
                })
                .Where(x => x.TotalRecipients > 1)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            var totalCount = grouped.Count;
            var items = grouped
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (items, totalCount);
        }
    }
}

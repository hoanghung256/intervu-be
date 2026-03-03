using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Domain.Repositories
{
    public interface INotificationRepository : IRepositoryBase<Notification>
    {
        Task<(IReadOnlyList<Notification> Items, int TotalCount)> GetByUserIdAsync(Guid userId, int page, int pageSize);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task MarkAsReadAsync(Guid notificationId);
        Task MarkAllAsReadAsync(Guid userId);
        Task<bool> ExistsAsync(Guid userId, NotificationType type, Guid referenceId);
    }
}

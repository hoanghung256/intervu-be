using Intervu.Application.DTOs.Notification;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.Notification;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.Notification
{
    public class NotificationUseCase : INotificationUseCase
    {
        private readonly INotificationRepository _notificationRepo;
        private readonly IInterviewRoomRepository _roomRepo;
        private readonly IUserRepository _userRepo;
        private readonly INotificationPusher _pusher;

        private const int BroadcastBatchSize = 500;

        public NotificationUseCase(
            INotificationRepository notificationRepo,
            IInterviewRoomRepository roomRepo,
            IUserRepository userRepo,
            INotificationPusher pusher)
        {
            _notificationRepo = notificationRepo;
            _roomRepo = roomRepo;
            _userRepo = userRepo;
            _pusher = pusher;
        }

        public async Task CreateAsync(Guid userId, NotificationType type, string title, string message,
            string? actionUrl = null, Guid? referenceId = null)
        {
            // Dedup check
            if (referenceId.HasValue &&
                await _notificationRepo.ExistsAsync(userId, type, referenceId.Value))
                return;

            var notification = new Domain.Entities.Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = type,
                Title = title,
                Message = message,
                ActionUrl = actionUrl,
                ReferenceId = referenceId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepo.AddAsync(notification);
            await _notificationRepo.SaveChangesAsync();

            // Push real-time via SignalR
            await _pusher.PushToUserAsync(userId, new NotificationDto
            {
                Id = notification.Id,
                Type = type.ToString(),
                Title = title,
                Message = message,
                ActionUrl = actionUrl,
                IsRead = false,
                CreatedAt = notification.CreatedAt
            });
        }

        public async Task CreateForMultipleUsersAsync(List<Guid> userIds, NotificationType type,
            string title, string message, string? actionUrl = null, Guid? referenceId = null)
        {
            var createdAt = DateTime.UtcNow;
            var notifications = new List<Domain.Entities.Notification>();

            foreach (var userId in userIds)
            {
                // Dedup check per user
                if (referenceId.HasValue &&
                    await _notificationRepo.ExistsAsync(userId, type, referenceId.Value))
                    continue;

                notifications.Add(new Domain.Entities.Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Type = type,
                    Title = title,
                    Message = message,
                    ActionUrl = actionUrl,
                    ReferenceId = referenceId,
                    IsRead = false,
                    CreatedAt = createdAt
                });
            }

            if (notifications.Count == 0) return;

            // Batch insert
            await _notificationRepo.AddRangeAsync(notifications);
            await _notificationRepo.SaveChangesAsync();

            // Push real-time to each user
            foreach (var noti in notifications)
            {
                await _pusher.PushToUserAsync(noti.UserId, new NotificationDto
                {
                    Id = noti.Id,
                    Type = type.ToString(),
                    Title = title,
                    Message = message,
                    ActionUrl = actionUrl,
                    IsRead = false,
                    CreatedAt = createdAt
                });
            }
        }

        public async Task BroadcastToAllAsync(NotificationType type,
            string title, string message, string? actionUrl = null)
        {
            await BroadcastAsync(role: null, type, title, message, actionUrl);
        }

        public async Task BroadcastToRoleAsync(string role, NotificationType type,
            string title, string message, string? actionUrl = null)
        {
            await BroadcastAsync(role, type, title, message, actionUrl);
        }

        /// <summary>
        /// Fetches all matching user IDs in batches and inserts notifications in bulk.
        /// A single SignalR push is performed after DB inserts (Clients.All or Clients.Group).
        /// </summary>
        private async Task BroadcastAsync(string? role, NotificationType type,
            string title, string message, string? actionUrl)
        {
            var userRole = role != null && Enum.TryParse<UserRole>(role, out var parsed) ? parsed : (UserRole?)null;
            var createdAt = DateTime.UtcNow;

            int page = 1;
            bool hasMore = true;

            while (hasMore)
            {
                var (users, _) = await _userRepo.GetPagedUsersByFilterAsync(page, BroadcastBatchSize, userRole, null);
                if (!users.Any()) break;

                var notifications = users.Select(u => new Domain.Entities.Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = u.Id,
                    Type = type,
                    Title = title,
                    Message = message,
                    ActionUrl = actionUrl,
                    IsRead = false,
                    CreatedAt = createdAt
                }).ToList();

                await _notificationRepo.AddRangeAsync(notifications);
                await _notificationRepo.SaveChangesAsync();

                hasMore = users.Count == BroadcastBatchSize;
                page++;
            }

            // Single SignalR push after all DB inserts
            var dto = new NotificationDto
            {
                Id = Guid.NewGuid(), // transient ID for FE display
                Type = type.ToString(),
                Title = title,
                Message = message,
                ActionUrl = actionUrl,
                IsRead = false,
                CreatedAt = createdAt
            };

            if (role == null)
                await _pusher.PushToAllAsync(dto);
            else
                await _pusher.PushToRoleGroupAsync(role, dto);
        }

        public async Task<NotificationListResponseDto> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            var (items, totalCount) = await _notificationRepo.GetByUserIdAsync(userId, page, pageSize);
            var unreadCount = await _notificationRepo.GetUnreadCountAsync(userId);

            return new NotificationListResponseDto
            {
                Items = items.Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Type = n.Type.ToString(),
                    Title = n.Title,
                    Message = n.Message,
                    ActionUrl = n.ActionUrl,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                }).ToList(),
                TotalCount = totalCount,
                UnreadCount = unreadCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _notificationRepo.GetUnreadCountAsync(userId);
        }

        public async Task MarkAsReadAsync(Guid notificationId, Guid currentUserId)
        {
            var notification = await _notificationRepo.GetByIdAsync(notificationId);
            if (notification == null || notification.UserId != currentUserId)
                return; // Silently ignore if not found or not owned by current user

            await _notificationRepo.MarkAsReadAsync(notificationId);
        }

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            await _notificationRepo.MarkAllAsReadAsync(userId);
        }

        public async Task SendInterviewReminderAsync(Guid roomId)
        {
            var room = await _roomRepo.GetByIdAsync(roomId);
            if (room == null || room.Status != InterviewRoomStatus.Scheduled)
                return;

            var scheduledTime = room.ScheduledTime?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";

            // Notify candidate
            if (room.CandidateId.HasValue)
            {
                var coach = room.CoachId.HasValue
                    ? await _userRepo.GetByIdAsync(room.CoachId.Value)
                    : null;

                await CreateAsync(
                    room.CandidateId.Value,
                    NotificationType.InterviewReminder,
                    "Interview starting soon",
                    $"Your interview with {coach?.FullName ?? "Coach"} starts at {scheduledTime}.",
                    "/interview?tab=upcoming",
                    roomId);
            }

            // Notify coach
            if (room.CoachId.HasValue)
            {
                var candidate = room.CandidateId.HasValue
                    ? await _userRepo.GetByIdAsync(room.CandidateId.Value)
                    : null;

                await CreateAsync(
                    room.CoachId.Value,
                    NotificationType.InterviewReminder,
                    "Interview starting soon",
                    $"Your interview with {candidate?.FullName ?? "Candidate"} starts at {scheduledTime}.",
                    "/interview?tab=upcoming",
                    roomId);
            }
        }
    }
}
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
            foreach (var userId in userIds)
            {
                await CreateAsync(userId, type, title, message, actionUrl, referenceId);
            }
        }

        public async Task<NotificationListResponse> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            var (items, totalCount) = await _notificationRepo.GetByUserIdAsync(userId, page, pageSize);
            var unreadCount = await _notificationRepo.GetUnreadCountAsync(userId);

            return new NotificationListResponse
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

        public async Task MarkAsReadAsync(Guid notificationId)
        {
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

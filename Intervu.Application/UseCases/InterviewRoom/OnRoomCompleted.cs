using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Application.Interfaces.UseCases.Notification;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Intervu.Application.UseCases.InterviewRoom
{
    public class OnRoomCompleted : IOnRoomCompleted
    {
        private readonly IInterviewRoomRepository _roomRepo;
        private readonly INotificationUseCase _notifications;
        private readonly ILogger<OnRoomCompleted> _logger;

        public OnRoomCompleted(
            IInterviewRoomRepository roomRepo,
            INotificationUseCase notifications,
            ILogger<OnRoomCompleted> logger)
        {
            _roomRepo = roomRepo;
            _notifications = notifications;
            _logger = logger;
        }

        public async Task ExecuteAsync(Guid roomId)
        {
            var room = await _roomRepo.GetByIdWithDetailsAsync(roomId);
            if (room == null)
            {
                _logger.LogWarning("OnRoomCompleted: room {RoomId} not found", roomId);
                return;
            }

            if (room.CoachId.HasValue)
            {
                await _notifications.CreateAsync(
                    room.CoachId.Value,
                    NotificationType.AiAnalysisStarted,
                    "Analyzing questions",
                    "We're extracting questions from your session.",
                    $"/interview?roomId={roomId}&action=review-questions",
                    roomId);
            }

            if (room.CandidateId.HasValue)
            {
                await _notifications.CreateAsync(
                    room.CandidateId.Value,
                    NotificationType.RoadmapUpdateStarted,
                    "Updating your roadmap",
                    "Your interview just ended. We're recalculating your roadmap.",
                    "/roadmap",
                    roomId);
            }
        }
    }
}

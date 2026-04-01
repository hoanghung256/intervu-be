using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Application.Interfaces.UseCases.Notification;

namespace Intervu.Application.UseCases.InterviewRoom
{
    public class ScheduleInterviewReminders : IScheduleInterviewReminders
    {
        private readonly IBackgroundService _backgroundService;

        public ScheduleInterviewReminders(IBackgroundService backgroundService)
        {
            _backgroundService = backgroundService;
        }

        public void Schedule(Guid roomId, DateTime scheduledTime)
        {
            var now = DateTime.UtcNow;

            // 4 reminder milestones before the interview
            var milestones = new[]
            {
                scheduledTime.AddDays(-1),      // 1 day before
                scheduledTime.AddHours(-12),    // 12 hours before
                scheduledTime.AddHours(-1),     // 1 hour before
                scheduledTime.AddMinutes(-5),   // 5 minutes before (final reminder to join)
            };

            foreach (var triggerTime in milestones)
            {
                // Only schedule reminders that haven't passed yet
                if (triggerTime > now)
                {
                    var delay = triggerTime - now;
                    _backgroundService.Schedule<INotificationUseCase>(
                        uc => uc.SendInterviewReminderAsync(roomId),
                        delay);
                }
            }
        }
    }
}

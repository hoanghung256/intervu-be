using Hangfire;
using Intervu.Application.Interfaces.BackgroundJobs;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.Notification;
using Intervu.Application.Services;
using Intervu.Domain.Entities.Constants;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Intervu.Infrastructure.BackgroundJobs
{
    public class InterviewMonitorJob : IRecurringJob
    {
        private readonly IntervuPostgreDbContext _db;
        private readonly InterviewRoomCache _cache;
        private readonly ILogger<InterviewMonitorJob> _logger;
        private readonly IBackgroundService _backgroundService;

        public InterviewMonitorJob(
            IntervuPostgreDbContext db,
            InterviewRoomCache cache,
            ILogger<InterviewMonitorJob> logger,
            IBackgroundService backgroundService)
        {
            _db = db;
            _cache = cache;
            _logger = logger;
            _backgroundService = backgroundService;
        }

        public string JobId => "InterviewMonitor";
        public string CronExpression => Cron.Minutely();

        // This job runs every minute to:
        // 1. Move rooms from Scheduled to Ongoing if their scheduled time is within the next 5 minutes.
        // 2. (Optional) Move rooms from Ongoing to Completed if they have been ongoing for more than 1 hour. 
        // This part is currently not implemented to avoid edge cases with interviews that run longer than expected. 
        public async Task ExecuteAsync()
        {
            var now = DateTime.UtcNow;

            // Move rooms starting within 5 minutes to Ongoing.
            var roomsToUpdate = await _db.InterviewRooms
                .Where(room => room.Status == InterviewRoomStatus.Scheduled &&
                               room.ScheduledTime.HasValue &&
                               room.ScheduledTime.Value <= now.AddMinutes(5) &&
                               room.ScheduledTime.Value > now)
                .ToListAsync();

            if (roomsToUpdate.Count != 0)
            {
                foreach (var room in roomsToUpdate)
                {
                    room.Status = InterviewRoomStatus.Ongoing;
                    _cache.Update(room);
                }

                _db.InterviewRooms.UpdateRange(roomsToUpdate);
                await _db.SaveChangesAsync();
                _logger.LogInformation("Changed rooms to Ongoing: {RoomIds}", string.Join(", ", roomsToUpdate.Select(r => r.Id)));

                // Queue reminder notifications for both participants.
                foreach (var room in roomsToUpdate)
                {
                    _backgroundService.Enqueue<INotificationUseCase>(
                        uc => uc.SendInterviewReminderAsync(room.Id));
                }
            }
        }
    }
}
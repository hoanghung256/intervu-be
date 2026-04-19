using Hangfire;
using Intervu.Application.Interfaces.BackgroundJobs;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;
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

        // Runs every minute to keep interview room status in sync with schedule.
        public async Task ExecuteAsync()
        {
            var now = DateTime.UtcNow;

            await MoveScheduledRoomsToOngoingAsync(now);
            await CompleteOverdueScheduledRoomsAsync(now);
            await CompleteOverdueOngoingRoomsAsync(now);
        }

        private async Task MoveScheduledRoomsToOngoingAsync(DateTime now)
        {
            // Move rooms starting within 5 minutes to Ongoing.
            var roomsToUpdate = await _db.InterviewRooms
                .Where(room => room.Status == InterviewRoomStatus.Scheduled &&
                               room.ScheduledTime.HasValue &&
                               room.ScheduledTime.Value <= now.AddMinutes(5) &&
                               room.ScheduledTime.Value > now)
                .ToListAsync();

            if (roomsToUpdate.Count == 0)
            {
                return;
            }

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

        private async Task CompleteOverdueScheduledRoomsAsync(DateTime now)
        {
            // Complete Scheduled rooms that passed planned end time.
            var scheduledRooms = await _db.InterviewRooms
                .Where(room => room.Status == InterviewRoomStatus.Scheduled &&
                               room.ScheduledTime.HasValue &&
                               room.DurationMinutes.HasValue)
                .ToListAsync();

            var roomsToComplete = scheduledRooms
                .Where(room => room.ScheduledTime!.Value
                    .AddMinutes(room.DurationMinutes!.Value) < now)
                .ToList();

            if (roomsToComplete.Count == 0)
            {
                return;
            }

            foreach (var room in roomsToComplete)
            {
                room.Status = InterviewRoomStatus.Completed;
                _cache.Update(room);
            }

            _db.InterviewRooms.UpdateRange(roomsToComplete);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Changed overdue Scheduled rooms to Completed: {RoomIds}", string.Join(", ", roomsToComplete.Select(r => r.Id)));

            // Queue payout processing for rooms completed in this run.
            foreach (var room in roomsToComplete)
            {
                _backgroundService.Enqueue<IPayoutForCoachAfterInterview>(
                    uc => uc.ExecuteAsync(room.Id));
            }
        }

        private async Task CompleteOverdueOngoingRoomsAsync(DateTime now)
        {
            // Complete rooms that are overdue by 60 minutes after planned end.
            var ongoingRooms = await _db.InterviewRooms
                .Where(room => room.Status == InterviewRoomStatus.Ongoing &&
                               room.ScheduledTime.HasValue &&
                               room.DurationMinutes.HasValue)
                .ToListAsync();

            var roomsToComplete = ongoingRooms
                .Where(room => room.ScheduledTime!.Value
                    .AddMinutes(room.DurationMinutes!.Value + 60) <= now)
                .ToList();

            if (roomsToComplete.Count == 0)
            {
                return;
            }

            foreach (var room in roomsToComplete)
            {
                room.Status = InterviewRoomStatus.Completed;
                _cache.Update(room);
            }

            _db.InterviewRooms.UpdateRange(roomsToComplete);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Changed rooms to Completed: {RoomIds}", string.Join(", ", roomsToComplete.Select(r => r.Id)));

            // Queue payout processing for rooms completed in this run.
            foreach (var room in roomsToComplete)
            {
                _backgroundService.Enqueue<IPayoutForCoachAfterInterview>(
                    uc => uc.ExecuteAsync(room.Id));
            }
        }
    }
}
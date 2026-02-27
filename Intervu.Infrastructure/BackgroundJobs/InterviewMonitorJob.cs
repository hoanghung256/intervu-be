using Hangfire;
using Intervu.Application.Interfaces.BackgroundJobs;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;
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
        private readonly IPayoutForCoachAfterInterview _payout;
        private readonly ILogger<InterviewMonitorJob> _logger;

        public InterviewMonitorJob(
            IntervuPostgreDbContext db,
            InterviewRoomCache cache,
            IPayoutForCoachAfterInterview payout,
            ILogger<InterviewMonitorJob> logger)
        {
            _db = db;
            _cache = cache;
            _payout = payout;
            _logger = logger;
        }

        public string JobId => "InterviewMonitor";
        public string CronExpression => Cron.Minutely();

        public async Task ExecuteAsync()
        {
            var now = DateTime.UtcNow;

            // 1. Update Scheduled -> Ongoing
            // Condition: ScheduledTime <= now + 5 mins AND ScheduledTime > now
            var roomsToUpdate = await _db.InterviewRooms
                .Where(room => room.Status == InterviewRoomStatus.Scheduled &&
                               room.ScheduledTime.HasValue &&
                               room.ScheduledTime.Value <= now.AddMinutes(5) &&
                               room.ScheduledTime.Value > now)
                .ToListAsync();

            if (roomsToUpdate.Any())
            {
                foreach (var room in roomsToUpdate)
                {
                    room.Status = InterviewRoomStatus.Ongoing;
                    _cache.Update(room);
                }
                
                _db.InterviewRooms.UpdateRange(roomsToUpdate);
                await _db.SaveChangesAsync();
                _logger.LogInformation("Changed rooms to Ongoing: {RoomIds}", string.Join(", ", roomsToUpdate.Select(r => r.Id)));
            }
        }
    }
}
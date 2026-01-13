using Intervu.Application.Interfaces.UseCases.InterviewBooking;
using Intervu.Application.Services;
using Intervu.Application.UseCases.InterviewBooking;
using Intervu.Domain.Entities.Constants;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Intervu.Infrastructure.Persistence.SqlServer.DataContext;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Infrastructure.ExternalServices
{
    public class InterviewMonitorService : BackgroundService
    {
        private readonly InterviewRoomCache _cache;
        private readonly IServiceProvider _services;

        public InterviewMonitorService(InterviewRoomCache cache, IServiceProvider services)
        {
            _cache = cache;
            _services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IntervuPostgreDbContext>();

                var roomsToUpdate = db.InterviewRooms
                .Where(room => room.Status == InterviewRoomStatus.Scheduled &&
                               room.ScheduledTime.HasValue &&
                               room.ScheduledTime.Value <= now.AddMinutes(5) &&
                               room.ScheduledTime.Value > now)
                .ToList();

                var roomsToEnd = db.InterviewRooms
                .Where(room => room.Status == InterviewRoomStatus.Ongoing &&
                               room.ScheduledTime.HasValue &&
                               now >= room.ScheduledTime.Value
                                            .AddHours(1)
                                            .AddMinutes(5))
                .ToList();

                if (roomsToUpdate.Any())
                {

                    foreach (var room in roomsToUpdate)
                    {
                        // Update status in DB
                        room.Status = InterviewRoomStatus.Ongoing;
                        db.InterviewRooms.Update(room);

                        // Update status in cache
                        _cache.Update(room);
                    }

                    await db.SaveChangesAsync(stoppingToken);
                    Console.WriteLine("Successfully change rooms status: " +
    string.Join(", ", roomsToUpdate.Select(r => $"RoomId: {r.Id}, Status: {r.Status}")));
                }

                if (roomsToEnd.Any())
                {
                    //using var scope = _services.CreateScope();
                    //var db = scope.ServiceProvider.GetRequiredService<IntervuDbContext>();
                    var payout = scope.ServiceProvider.GetRequiredService<Intervu.Application.Interfaces.UseCases.InterviewBooking.IPayoutForCoachAfterInterview>();

                    foreach (var room in roomsToEnd)
                    {
                        // Update status in DB
                        room.Status = InterviewRoomStatus.Completed;
                        db.InterviewRooms.Update(room);

                        await payout.ExecuteAsync(room.Id);

                        // Update status in cache
                        _cache.Update(room);
                    }

                    await db.SaveChangesAsync(stoppingToken);
                    Console.WriteLine("Successfully change rooms status: " +
    string.Join(", ", roomsToEnd.Select(r => $"RoomId: {r.Id}, Status: {r.Status}")));
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}

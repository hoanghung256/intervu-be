using Intervu.Application.Services;
using Intervu.Domain.Entities.Constants;
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
        private readonly RoomManagerService _cache;
        private readonly IServiceProvider _services;

        public InterviewMonitorService(RoomManagerService cache, IServiceProvider services)
        {
            _cache = cache;
            _services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;

                var roomsToUpdate = _cache.Rooms
                .Where(room => room.Status == InterviewRoomStatus.Scheduled &&
                               room.ScheduledTime.HasValue &&
                               room.ScheduledTime.Value <= now.AddMinutes(5) &&
                               room.ScheduledTime.Value > now)
                .ToList();

                if (roomsToUpdate.Any())
                {
                    using var scope = _services.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<IntervuDbContext>();

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

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}

using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Application.Services;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Intervu.Infrastructure.Persistence.SqlServer.DataContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Infrastructure.ExternalServices
{
    public class InterviewRoomCacheLoader : IHostedService
    {
        private readonly IServiceProvider _services;
        private readonly InterviewRoomCache _cache;

        public InterviewRoomCacheLoader(IServiceProvider services, InterviewRoomCache cache)
        {
            _services = services;
            _cache = cache;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IntervuPostgreDbContext>();

            var rooms = await db.InterviewRooms.ToListAsync(cancellationToken);
            _cache.SetAll(rooms);
            Console.WriteLine("Successfully add room to cache: " +
    string.Join(", ", rooms.Select(r => $"RoomId: {r.Id}, Status: {r.Status}")));
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}

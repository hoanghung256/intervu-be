using Hangfire;
using Intervu.Application.Interfaces.BackgroundJobs;
using Intervu.Application.Interfaces.UseCases.BookingRequest;
using Microsoft.Extensions.Logging;

namespace Intervu.Infrastructure.BackgroundJobs
{
    public class BookingExpireJob : IRecurringJob
    {
        private readonly IExpireBookingRequests _expireBookingRequests;
        private readonly ILogger<BookingExpireJob> _logger;

        public BookingExpireJob(IExpireBookingRequests expireBookingRequests, ILogger<BookingExpireJob> logger)
        {
            _expireBookingRequests = expireBookingRequests;
            _logger = logger;
        }

        public string JobId => "BookingExpire";
        public string CronExpression => Cron.Minutely();

        public async Task ExecuteAsync()
        {
            var count = await _expireBookingRequests.ExecuteAsync();
            if (count > 0)
                _logger.LogInformation("Expired {Count} booking request(s)", count);
        }
    }
}

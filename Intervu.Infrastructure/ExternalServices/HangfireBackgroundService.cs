using Intervu.Application.Interfaces.ExternalServices;
using System.Linq.Expressions;
using Hangfire;

namespace Intervu.Infrastructure.ExternalServices
{
    internal class HangfireBackgroundService : IBackgroundService
    {
        public void AddOrUpdateRecurring<T>(Guid jobId, Expression<Action<T>> methodCall, string cronExpression)
        {
            RecurringJob.AddOrUpdate<T>(jobId.ToString(), methodCall, cronExpression);
        }

        public void Enqueue<T>(Expression<Action<T>> methodCall)
        {
            BackgroundJob.Enqueue<T>(methodCall);
        }

        public void Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay)
        {
            BackgroundJob.Schedule<T>(methodCall, delay);
        }
    }
}

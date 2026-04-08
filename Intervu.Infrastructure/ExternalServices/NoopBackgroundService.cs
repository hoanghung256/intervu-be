using System;
using System.Linq.Expressions;
using Intervu.Application.Interfaces.ExternalServices;

namespace Intervu.Infrastructure.ExternalServices
{
    internal class NoopBackgroundService : IBackgroundService
    {
        public void AddOrUpdateRecurring<T>(Guid jobId, Expression<Action<T>> methodCall, string cronExpression)
        {
        }

        public void Enqueue<T>(Expression<Action<T>> methodCall)
        {
        }

        public void Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay)
        {
        }

        public bool Delete(string jobId)
        {
            return false;
        }

        public void RemoveRecurring(Guid jobId)
        {
        }
    }
}

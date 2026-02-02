using System;
using System.Linq.Expressions;

namespace Intervu.Application.Interfaces.ExternalServices
{
    /// <summary>
    /// Provides a generic way to manage background tasks without depending directly on Hangfire.
    /// </summary>
    public interface IBackgroundService
    {
        /// <summary>
        /// Executes a task immediately in the background. (Fire-and-Forget)
        /// </summary>
        /// <param name="methodCall">The method to run. Example: () => _service.DoWork()</param>
        void Enqueue<T>(Expression<Action<T>> methodCall);

        /// <summary>
        /// Schedules a task to run once after a specific delay. (Delayed Job)
        /// </summary>
        /// <param name="methodCall">The method to run.</param>
        /// <param name="delay">The time to wait before execution.</param>
        void Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay);

        /// <summary>
        /// Creates or updates a task that runs repeatedly on a schedule. (Recurring Job)
        /// </summary>
        /// <param name="jobId">A unique ID for this job. Use the same ID to update or overwrite an existing job.</param>
        /// <param name="methodCall">The method to run.</param>
        /// <param name="cronExpression">A CRON string defining the frequency. Example: "0 0 * * *" (Every midnight).</param>
        /// <remarks>
        /// IMPORTANT: Only pass simple types (int, string, Guid) as arguments in methodCall. 
        /// Avoid passing complex objects to prevent serialization issues.
        /// </remarks>
        void AddOrUpdateRecurring<T>(Guid jobId, Expression<Action<T>> methodCall, string cronExpression);
    }
}
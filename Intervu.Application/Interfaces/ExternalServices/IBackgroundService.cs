using System;
using System.Linq.Expressions;

namespace Intervu.Application.Interfaces.ExternalServices
{
    /// <summary>
    /// Provides a generic abstraction for managing background tasks.
    /// This keeps the Application layer decoupled from specific libraries like Hangfire.
    /// </summary>
    public interface IBackgroundService
    {
        /// <summary>
        /// Enqueues a task for immediate execution in the background. (Fire-and-Forget)
        /// </summary>
        /// <typeparam name="T">The service type containing the logic to execute.</typeparam>
        /// <param name="methodCall">A lambda expression trigerring the service method.</param>
        /// <example>
        /// <code>
        /// _backgroundService.Enqueue&lt;IEmailService&gt;(x => x.SendWelcomeEmail(user.Id));
        /// </code>
        /// </example>
        void Enqueue<T>(Expression<Action<T>> methodCall);

        /// <summary>
        /// Schedules a task to run once after a specified delay. (Delayed Job)
        /// </summary>
        /// <typeparam name="T">The service type containing the logic to execute.</typeparam>
        /// <param name="methodCall">A lambda expression triggering the service method.</param>
        /// <param name="delay">The time span to wait before execution.</param>
        /// <example>
        /// <code>
        /// // Set coach availability to 'Available' after a 5-minute break
        /// _backgroundService.Schedule&lt;ICoachService&gt;(
        ///     x => x.SetAvailable(availabilityId), 
        ///     TimeSpan.FromMinutes(5));
        /// </code>
        /// </example>
        void Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay);

        /// <summary>
        /// Creates or updates a task to run repeatedly on a specific schedule. (Recurring Job)
        /// </summary>
        /// <typeparam name="T">The service type containing the logic to execute.</typeparam>
        /// <param name="jobId">A unique identifier for the job. Use a fixed ID to prevent duplicates or to update an existing schedule.</param>
        /// <param name="methodCall">A lambda expression triggering the service method.</param>
        /// <param name="cronExpression">A CRON string defining the frequency (e.g., "0 0 * * *" for daily at midnight).</param>
        /// <remarks>
        /// <para><strong>IMPORTANT:</strong></para>
        /// <list type="bullet">
        /// <item>Only pass primitive or simple types (Guid, int, string) as method arguments.</item>
        /// <item>Avoid passing complex objects (Entities, DTOs) to prevent serialization failures.</item>
        /// <item>The service <typeparamref name="T"/> must be registered in the Dependency Injection container.</item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Generate interview reports every night at 11 PM
        /// _backgroundService.AddOrUpdateRecurring&lt;IReportService&gt;(
        ///     Guid.Parse("D46B7085-A4E3-4F8A-9A2E-8E0E87F6A5D2"),
        ///     x => x.GenerateDailySystemReport(),
        ///     "0 23 * * *");
        /// </code>
        /// </example>
        void AddOrUpdateRecurring<T>(Guid jobId, Expression<Action<T>> methodCall, string cronExpression);
    }
}
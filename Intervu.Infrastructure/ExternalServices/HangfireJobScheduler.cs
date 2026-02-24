﻿using Hangfire;
using Intervu.Application.Interfaces.BackgroundJobs;
using System.Linq.Expressions;
using System.Reflection;

namespace Intervu.Infrastructure.ExternalServices
{
    public class HangfireJobScheduler
    {
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly IEnumerable<IRecurringJob> _recurringJobs;

        public HangfireJobScheduler(IRecurringJobManager recurringJobManager, IEnumerable<IRecurringJob> recurringJobs)
        {
            _recurringJobManager = recurringJobManager;
            _recurringJobs = recurringJobs;
        }

        public void RegisterRecurringJobs()
        {
            foreach (var job in _recurringJobs)
            {
                var jobType = job.GetType();
                var jobId = job.JobId;
                var cronExpression = job.CronExpression;

                // We use reflection to invoke the generic AddOrUpdate<T> extension method
                // This allows Hangfire to resolve T from the IoC container when the job triggers
                var method = typeof(RecurringJobManagerExtensions)
                    .GetMethods(BindingFlags.Static | BindingFlags.Public)
                    .Where(m => m.Name == "AddOrUpdate" && m.IsGenericMethod && m.GetParameters().Length == 6)
                    .FirstOrDefault(m =>
                    {
                        var parameters = m.GetParameters();

                        // Check #1: The method call (param index 2) must be an async task.
                        // We need Expression<Func<T, Task>>, where the delegate has 2 generic args (T, Task).
                        var expressionType = parameters[2].ParameterType;
                        if (!expressionType.IsGenericType) return false;
 
                        var genericArgs = expressionType.GetGenericArguments();
                        if (genericArgs.Length == 0) return false;
 
                        var delegateType = genericArgs[0];
                        bool isAsyncDelegate = delegateType.IsGenericType && delegateType.GetGenericArguments().Length == 2;

                        // Check #2: The cron expression (param index 3) must be a string, not a Func<string>.
                        bool isStringCron = parameters[3].ParameterType == typeof(string);

                        return isAsyncDelegate && isStringCron;
                    });

                if (method != null)
                {
                    var genericMethod = method.MakeGenericMethod(jobType);
                    var param = Expression.Parameter(jobType, "x");
                    var executeMethod = jobType.GetMethod(nameof(IRecurringJob.ExecuteAsync));
                    var body = Expression.Call(param, executeMethod!);
                    var lambda = Expression.Lambda(body, param);

                    // Invoke: AddOrUpdate<T>(manager, jobId, x => x.ExecuteAsync(), cron, timeZone, queue)
                    genericMethod.Invoke(null, new object?[] { _recurringJobManager, jobId, lambda, cronExpression, null, "default" });
                }
            }
        }
    }
}
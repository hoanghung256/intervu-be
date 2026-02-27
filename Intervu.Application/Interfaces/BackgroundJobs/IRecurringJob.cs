using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.BackgroundJobs
{
    public interface IRecurringJob
    {
        string JobId { get; }
        string CronExpression { get; }
        Task ExecuteAsync();
    }
}
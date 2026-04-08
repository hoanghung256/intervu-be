using System;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Email
{
    public interface ISendInterviewReminderEmail
    {
        Task ExecuteAsync(Guid roomId, string timeUntil);
    }
}

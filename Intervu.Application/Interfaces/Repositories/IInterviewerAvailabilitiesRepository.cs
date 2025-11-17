using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Domain.Entities;

namespace Intervu.Application.Interfaces.Repositories
{
    public interface IInterviewerAvailabilitiesRepository
    {
        Task<bool> IsInterviewerAvailableAsync(int interviewerId, DateTimeOffset startTime, DateTimeOffset endTime);
        Task<IEnumerable<InterviewerAvailability>> GetInterviewerAvailabilitiesByMonthAsync (int intervewerId, int month = 0, int year = 0);
    }
}

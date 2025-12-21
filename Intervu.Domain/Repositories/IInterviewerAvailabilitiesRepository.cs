using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Domain.Entities;

namespace Intervu.Domain.Repositories
{
    public interface IInterviewerAvailabilitiesRepository : IRepositoryBase<InterviewerAvailability>
    {
        Task<bool> IsInterviewerAvailableAsync(Guid interviewerId, DateTimeOffset startTime, DateTimeOffset endTime);
        Task<IEnumerable<InterviewerAvailability>> GetInterviewerAvailabilitiesByMonthAsync (Guid intervewerId, int month = 0, int year = 0);
        Task<Guid> CreateInterviewerAvailabilityAsync(InterviewerAvailability availability);
        Task<Guid> CreateMultipleInterviewerAvailabilitiesAsync(List<InterviewerAvailability> availabilities);
        Task<bool> DeleteInterviewerAvailabilityAsync(Guid availabilityId);
        Task<bool> UpdateInterviewerAvailabilityAsync(Guid availabilityId, DateTimeOffset startTime, DateTimeOffset endTime);

        Task<InterviewerAvailability?> GetAsync(Guid interviewerId, DateTime startTime);
    }
}

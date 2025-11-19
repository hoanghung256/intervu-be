using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Application.DTOs.Availability;
using Intervu.Domain.Entities;

namespace Intervu.Application.Interfaces.Repositories
{
    public interface IInterviewerAvailabilitiesRepository : IRepositoryBase<InterviewerAvailability>
    {
        Task<bool> IsInterviewerAvailableAsync(int interviewerId, DateTimeOffset startTime, DateTimeOffset endTime);
        Task<IEnumerable<InterviewerAvailability>> GetInterviewerAvailabilitiesByMonthAsync (int intervewerId, int month = 0, int year = 0);
        Task<int> CreateInterviewerAvailabilityAsync(InterviewerAvailability availability);
        Task<int> CreateMultipleInterviewerAvailabilitiesAsync(List<InterviewerAvailability> availabilities);
        Task<bool> DeleteInterviewerAvailabilityAsync(int availabilityId);
        Task<bool> UpdateInterviewerAvailabilityAsync(int availabilityId, InterviewerAvailabilityUpdateDto dto);
    }
}

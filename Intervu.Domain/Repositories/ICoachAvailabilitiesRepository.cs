using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Domain.Repositories
{
    public interface ICoachAvailabilitiesRepository : IRepositoryBase<CoachAvailability>
    {
        Task<bool> IsCoachAvailableAsync(Guid coachId, DateTimeOffset startTime, DateTimeOffset endTime);
        Task<IEnumerable<CoachAvailability>> GetCoachAvailabilitiesByMonthAsync(Guid coachId, int month = 0, int year = 0);
        Task<Guid> CreateCoachAvailabilityAsync(CoachAvailability availability);
        Task<Guid> CreateMultipleCoachAvailabilitiesAsync(List<CoachAvailability> availabilities);
        Task<bool> DeleteCoachAvailabilityAsync(Guid availabilityId);
        Task<bool> UpdateCoachAvailabilityAsync(Guid availabilityId, InterviewFocus focus,DateTimeOffset startTime, DateTimeOffset endTime, Guid typeId);
        Task<CoachAvailability?> GetAsync(Guid coachId, DateTime startTime);
    }
}

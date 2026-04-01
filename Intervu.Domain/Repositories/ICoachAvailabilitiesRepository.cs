using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Domain.Repositories
{
    public interface ICoachAvailabilitiesRepository : IRepositoryBase<CoachAvailability>
    {
        Task<bool> IsCoachAvailableAsync(Guid coachId, DateTimeOffset startTime, DateTimeOffset endTime, Guid? excludeId = null);
        Task<IEnumerable<CoachAvailability>> GetCoachAvailabilitiesByMonthAsync(Guid coachId, int month = 0, int year = 0);
        Task<Guid> CreateCoachAvailabilityAsync(CoachAvailability availability);
        Task<Guid> CreateMultipleCoachAvailabilitiesAsync(List<CoachAvailability> availabilities);
        Task<bool> DeleteCoachAvailabilityAsync(Guid availabilityId);
        Task<bool> UpdateCoachAvailabilityAsync(Guid availabilityId, DateTimeOffset startTime, DateTimeOffset endTime);
        Task<CoachAvailability?> GetAsync(Guid coachId, DateTime startTime);
        /// <summary>
        /// Finds a single Available CoachAvailability whose range fully contains [startTime, endTime].
        /// </summary>
        Task<CoachAvailability?> FindContainingAvailabilityAsync(Guid coachId, DateTime startTime, DateTime endTime);

        /// <summary>
        /// Loads and locks an availability row for the lifetime of the current transaction.
        /// Used to serialize concurrent booking attempts on the same slot.
        /// </summary>
        Task<CoachAvailability?> GetByIdForUpdateAsync(Guid availabilityId);
    }
}

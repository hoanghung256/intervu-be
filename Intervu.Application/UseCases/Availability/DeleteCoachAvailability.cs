using System.Threading.Tasks;
using Intervu.Application.Interfaces.UseCases.Availability;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.Availability
{
    public class DeleteCoachAvailability : IDeleteCoachAvailability
    {
        private readonly ICoachAvailabilitiesRepository _repo;

        public DeleteCoachAvailability(ICoachAvailabilitiesRepository repo)
        {
            _repo = repo;
        }

        public async Task<bool> ExecuteAsync(Guid availabilityId)
        {
            if (availabilityId == Guid.Empty)
                throw new ArgumentException("Availability ID must be a valid GUID");

            var availability = await _repo.GetByIdAsync(availabilityId);
            if (availability == null)
                throw new InvalidOperationException("Availability not found");

            if (availability.Status != CoachAvailabilityStatus.Available)
                throw new ArgumentException("You can only delete available slots.");


            var deleted = await _repo.DeleteCoachAvailabilityAsync(availabilityId);
            if (!deleted)
                throw new InvalidOperationException($"Availability with ID {availabilityId} not found or could not be deleted");

            return true;
        }
    }
}

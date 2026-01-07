using System.Threading.Tasks;
using Intervu.Application.Interfaces.UseCases.Availability;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.Availability
{
    public class DeleteInterviewerAvailability : IDeleteInterviewerAvailability
    {
        private readonly IInterviewerAvailabilitiesRepository _repo;

        public DeleteInterviewerAvailability(IInterviewerAvailabilitiesRepository repo)
        {
            _repo = repo;
        }

        public async Task<bool> ExecuteAsync(Guid availabilityId)
        {
            if (availabilityId == Guid.Empty)
                throw new ArgumentException("Availability ID must be a valid GUID");

            var deleted = await _repo.DeleteInterviewerAvailabilityAsync(availabilityId);
            if (!deleted)
                throw new InvalidOperationException($"Availability with ID {availabilityId} not found or could not be deleted");

            return true;
        }
    }
}

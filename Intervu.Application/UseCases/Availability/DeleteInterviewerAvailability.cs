using System.Threading.Tasks;
using Intervu.Application.Interfaces.Repositories;
using Intervu.Application.Interfaces.UseCases.Availability;

namespace Intervu.Application.UseCases.Availability
{
    public class DeleteInterviewerAvailability : IDeleteInterviewerAvailability
    {
        private readonly IInterviewerAvailabilitiesRepository _repo;

        public DeleteInterviewerAvailability(IInterviewerAvailabilitiesRepository repo)
        {
            _repo = repo;
        }

        public async Task<bool> ExecuteAsync(int availabilityId)
        {
            if (availabilityId <= 0)
                throw new ArgumentException("Availability ID must be greater than 0");

            var deleted = await _repo.DeleteInterviewerAvailabilityAsync(availabilityId);
            if (!deleted)
                throw new InvalidOperationException($"Availability with ID {availabilityId} not found or could not be deleted");

            return true;
        }
    }
}

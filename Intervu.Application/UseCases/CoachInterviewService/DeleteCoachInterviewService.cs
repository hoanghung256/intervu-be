using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.UseCases.CoachInterviewService;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.CoachInterviewService
{
    internal class DeleteCoachInterviewService : IDeleteCoachInterviewService
    {
        private readonly ICoachInterviewServiceRepository _serviceRepo;

        public DeleteCoachInterviewService(ICoachInterviewServiceRepository serviceRepo)
        {
            _serviceRepo = serviceRepo;
        }

        public async Task ExecuteAsync(Guid coachId, Guid serviceId)
        {
            var service = await _serviceRepo.GetByIdAsync(serviceId)
                ?? throw new NotFoundException("Coach interview service not found");

            if (service.CoachId != coachId)
                throw new ForbiddenException("You can only delete your own interview services");

            _serviceRepo.DeleteAsync(service);
            await _serviceRepo.SaveChangesAsync();
        }
    }
}

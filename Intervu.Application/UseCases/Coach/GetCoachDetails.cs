using Intervu.Application.Interfaces.UseCases.Coach;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Coach
{
    public class GetCoachDetails : IGetCoachDetails
    {
        private readonly ICoachProfileRepository _coachProfileRepository;

        public GetCoachDetails(ICoachProfileRepository coachProfileRepository)
        {
            _coachProfileRepository = coachProfileRepository;
        }

        public async Task<User> ExecuteAsync(Guid coachId)
        {
            var coachProfile = await _coachProfileRepository.GetProfileByIdAsync(coachId);
            
            if (coachProfile == null)
                throw new InvalidOperationException($"Coach with ID {coachId} not found.");

            if (coachProfile.User == null)
                throw new InvalidOperationException($"User information for coach {coachId} is missing.");

            return coachProfile.User;
        }
    }
}

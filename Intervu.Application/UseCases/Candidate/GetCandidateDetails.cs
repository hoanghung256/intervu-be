using Intervu.Application.Interfaces.UseCases.Candidate;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.Candidate
{
    public class GetCandidateDetails : IGetCandidateDetails
    {
        private readonly IUserRepository _userRepository;

        public GetCandidateDetails(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> ExecuteAsync(Guid candidateId)
        {
            var user = await _userRepository.GetByIdAsync(candidateId);
            
            if (user == null)
                throw new InvalidOperationException($"Candidate with ID {candidateId} not found.");

            return user;
        }
    }
}

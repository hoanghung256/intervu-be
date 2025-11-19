using Intervu.Application.Interfaces.Repositories;
using Intervu.Application.Interfaces.UseCases.Interviewer;
using Intervu.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Interviewer
{
    public class GetInterviewerDetails : IGetInterviewerDetails
    {
        private readonly IInterviewerProfileRepository _interviewerProfileRepository;

        public GetInterviewerDetails(IInterviewerProfileRepository interviewerProfileRepository)
        {
            _interviewerProfileRepository = interviewerProfileRepository;
        }

        public async Task<User> ExecuteAsync(int interviewerId)
        {
            var interviewerProfile = await _interviewerProfileRepository.GetProfileByIdAsync(interviewerId);
            
            if (interviewerProfile == null)
                throw new InvalidOperationException($"Interviewer with ID {interviewerId} not found.");

            if (interviewerProfile.User == null)
                throw new InvalidOperationException($"User information for interviewer {interviewerId} is missing.");

            return interviewerProfile.User;
        }
    }
}

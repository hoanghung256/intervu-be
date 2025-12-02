using Intervu.Application.Interfaces.UseCases.Interviewee;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Interviewee
{
    public class GetIntervieweeDetails : IGetIntervieweeDetails
    {
        private readonly IUserRepository _userRepository;

        public GetIntervieweeDetails(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> ExecuteAsync(int intervieweeId)
        {
            var user = await _userRepository.GetByIdAsync(intervieweeId);
            
            if (user == null)
                throw new InvalidOperationException($"Interviewee with ID {intervieweeId} not found.");

            return user;
        }
    }
}

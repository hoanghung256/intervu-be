using Intervu.Application.Interfaces.UseCases.Admin;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.Admin
{
    public class DeleteUserForAdmin : IDeleteUserForAdmin
    {
        private readonly IUserRepository _userRepository;
        private readonly ICandidateProfileRepository _candidateProfileRepository;
        private readonly ICoachProfileRepository _coachProfileRepository;

        public DeleteUserForAdmin(
            IUserRepository userRepository,
            ICandidateProfileRepository candidateProfileRepository,
            ICoachProfileRepository coachProfileRepository)
        {
            _userRepository = userRepository;
            _candidateProfileRepository = candidateProfileRepository;
            _coachProfileRepository = coachProfileRepository;
        }

        public async Task<bool> ExecuteAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return false;

            user.Status = UserStatus.Suspended;
            _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            return true;
        }
    }
}

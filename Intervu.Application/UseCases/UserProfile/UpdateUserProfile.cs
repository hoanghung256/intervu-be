using AutoMapper;
using Intervu.Application.DTOs.User;
using Intervu.Application.Interfaces.UseCases.UserProfile;
using Intervu.Domain.Repositories;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.UserProfile
{
    public class UpdateUserProfile : IUpdateUserProfile
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UpdateUserProfile(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<UserDto?> ExecuteAsync(Guid userId, UpdateProfileRequest request)
        {
            var success = await _userRepository.UpdateProfileAsync(userId, request.FullName);
            
            if (!success)
            {
                return null;
            }

            var user = await _userRepository.GetByIdAsync(userId);
            return _mapper.Map<UserDto>(user);
        }
    }
}

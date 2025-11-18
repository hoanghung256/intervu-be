using AutoMapper;
using Intervu.Application.DTOs.User;
using Intervu.Application.Interfaces.Repositories;
using Intervu.Application.Interfaces.UseCases.UserProfile;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.UserProfile
{
    public class GetUserProfile : IGetUserProfile
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public GetUserProfile(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<UserDto?> ExecuteAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user == null)
            {
                return null;
            }

            return _mapper.Map<UserDto>(user);
        }
    }
}

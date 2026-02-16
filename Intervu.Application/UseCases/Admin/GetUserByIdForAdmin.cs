using AutoMapper;
using Intervu.Application.DTOs.Admin;
using Intervu.Application.Interfaces.UseCases.Admin;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.Admin
{
    public class GetUserByIdForAdmin : IGetUserByIdForAdmin
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public GetUserByIdForAdmin(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<AdminUserResponseDto?> ExecuteAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user == null)
                return null;

            return _mapper.Map<AdminUserResponseDto>(user);
        }
    }
}

using AutoMapper;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.Admin;
using Intervu.Domain.Repositories;
using Intervu.Application.Interfaces.UseCases.Admin;

namespace Intervu.Application.UseCases.Admin
{
    public class GetAllUsers : IGetAllUsersForAdmin
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public GetAllUsers(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<PagedResult<UserDto>> ExecuteAsync(int page, int pageSize)
        {
            var (items, total) = await _userRepository.GetPagedUsersAsync(page, pageSize);

            var userDtos = _mapper.Map<List<UserDto>>(items);

            return new PagedResult<UserDto>(userDtos, total, pageSize, page);
        }
    }
}

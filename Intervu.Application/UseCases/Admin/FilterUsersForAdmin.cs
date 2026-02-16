using AutoMapper;
using Intervu.Application.DTOs.Admin;
using Intervu.Application.DTOs.Common;
using Intervu.Application.Interfaces.UseCases.Admin;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.Admin
{
    public class FilterUsersForAdmin : IFilterUsersForAdmin
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public FilterUsersForAdmin(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<PagedResult<UserDto>> ExecuteAsync(int page, int pageSize, UserRole? role, string? search)
        {
            var (items, total) = await _userRepository.GetPagedUsersByFilterAsync(page, pageSize, role, search);

            var userDtos = _mapper.Map<List<UserDto>>(items);

            return new PagedResult<UserDto>(userDtos, total, pageSize, page);
        }
    }
}

using AutoMapper;
using Intervu.Application.Common;
using Intervu.Application.DTOs.Admin;
using Intervu.Application.Interfaces.Repositories;
using Intervu.Application.Interfaces.UseCases.Admin;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            var pagedUsers = await _userRepository.GetPagedUsersAsync(page, pageSize);

            var userDtos = _mapper.Map<List<UserDto>>(pagedUsers.Items);

            return new PagedResult<UserDto>(userDtos, pagedUsers.TotalItems, pageSize, page);
        }
    }
}

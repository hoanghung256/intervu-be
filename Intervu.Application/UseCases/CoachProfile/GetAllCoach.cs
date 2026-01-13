using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.Coach;
using Intervu.Application.DTOs.User;
using Intervu.Application.Interfaces.UseCases.CoachProfile;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.CoachProfile
{
    public class GetAllCoach : IGetAllCoach
    {
        private readonly ICoachProfileRepository _coachProfileRepo;
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;

        public GetAllCoach(ICoachProfileRepository coachProfileRepository, IUserRepository userRepository ,IMapper mapper)
        {
            _coachProfileRepo = coachProfileRepository;
            _userRepo = userRepository;
            _mapper = mapper;
        }

        public async Task<PagedResult<CoachProfileDto>> ExecuteAsync(GetCoachFilterRequest request)
        {
            var (items, total) = await _coachProfileRepo.GetPagedCoachProfilesAsync(
                request.Search,
                request.SkillId,
                request.CompanyId,
                request.Page,
                request.PageSize
            );

            List<CoachProfileDto> dtoList = _mapper.Map<List<CoachProfileDto>>(items);

            foreach (CoachProfileDto i in dtoList)
            {
                User? user = await _userRepo.GetByIdAsync(i.Id);
                i.User = _mapper.Map<UserDto>(user);
            }

            return new PagedResult<CoachProfileDto>
            (
                dtoList,
                total,
                request.PageSize,
                request.Page
            );
        }
    }
}

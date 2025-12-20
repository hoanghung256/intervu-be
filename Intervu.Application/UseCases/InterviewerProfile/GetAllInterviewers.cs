using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.Interviewer;
using Intervu.Application.DTOs.User;
using Intervu.Application.Interfaces.UseCases.InterviewerProfile;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.InterviewerProfile
{
    public class GetAllInterviewers : IGetAllInterviewers
    {
        private readonly IInterviewerProfileRepository _interviewerProfileRepo;
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;

        public GetAllInterviewers(IInterviewerProfileRepository interviewerProfileRepository, IUserRepository userRepository ,IMapper mapper)
        {
            _interviewerProfileRepo = interviewerProfileRepository;
            _userRepo = userRepository;
            _mapper = mapper;
        }

        public async Task<PagedResult<InterviewerProfileDto>> ExecuteAsync(GetInterviewerFilterRequest request)
        {
            var (items, total) = await _interviewerProfileRepo.GetPagedInterviewerProfilesAsync(
                request.Search,
                request.SkillId,
                request.CompanyId,
                request.Page,
                request.PageSize
            );

            List<InterviewerProfileDto> dtoList = _mapper.Map<List<InterviewerProfileDto>>(items);

            foreach (InterviewerProfileDto i in dtoList)
            {
                User user = await _userRepo.GetByIdAsync(i.Id);
                i.User = _mapper.Map<UserDto>(user);
            }

            return new PagedResult<InterviewerProfileDto>
            (
                dtoList,
                total,
                request.PageSize,
                request.Page
            );
        }
    }
}

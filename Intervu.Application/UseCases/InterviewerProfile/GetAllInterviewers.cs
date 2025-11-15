using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Intervu.Application.Common;
using Intervu.Application.DTOs.Interviewer;
using Intervu.Application.DTOs.User;
using Intervu.Application.Interfaces.Repositories;
using Intervu.Application.Interfaces.UseCases.InterviewerProfile;
using Intervu.Domain.Entities;

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

        public async Task<PagedResult<InterviewerProfileDto>> ExecuteAsync(int page, int pageSize)
        {
            PagedResult<Domain.Entities.InterviewerProfile> result = await _interviewerProfileRepo.GetPagedInterviewerProfilesAsync(page, pageSize);

            List<InterviewerProfileDto> dtoList = _mapper.Map<List<InterviewerProfileDto>>(result.Items);

            foreach (InterviewerProfileDto i in dtoList)
            {
                User user = await _userRepo.GetByIdAsync(i.Id);
                i.User = _mapper.Map<UserDto>(user);
            }

            return new PagedResult<InterviewerProfileDto>
            (
                dtoList,
                result.TotalItems,
                result.PageSize,
                result.CurrentPage
            );
        }
    }
}

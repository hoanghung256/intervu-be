using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Intervu.Application.Common;
using Intervu.Application.DTOs.Interviewer;
using Intervu.Application.Interfaces.Repositories;
using Intervu.Application.Interfaces.UseCases.InterviewerProfile;

namespace Intervu.Application.UseCases.InterviewerProfile
{
    public class GetAllInterviewers : IGetAllInterviewers
    {
        private readonly IInterviewerProfileRepository _repo;
        private readonly IMapper _mapper;

        public GetAllInterviewers(IInterviewerProfileRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<PagedResult<InterviewerProfileDto>> ExecuteAsync(int page, int pageSize)
        {
            var result = await _repo.GetPagedInterviewerProfilesAsync(page, pageSize);

            return new PagedResult<InterviewerProfileDto>
            (
                _mapper.Map<List<InterviewerProfileDto>>(result.Items),
                result.TotalItems,
                result.PageSize,
                result.CurrentPage
            );
        }
    }
}

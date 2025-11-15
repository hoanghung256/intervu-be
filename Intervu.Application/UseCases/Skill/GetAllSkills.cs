using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Intervu.Application.Common;
using Intervu.Application.DTOs.Skill;
using Intervu.Application.Interfaces.Repositories;
using Intervu.Application.Interfaces.UseCases.Skill;

namespace Intervu.Application.UseCases.Skill
{
    public class GetAllSkills : IGetAllSkills
    {
        private readonly ISkillRepository _skillRepository;
        private readonly IMapper _mapper;
        public GetAllSkills(ISkillRepository skillRepository, IMapper mapper)
        {
            _skillRepository = skillRepository;
            _mapper = mapper;
        }
        public async Task<PagedResult<SkillDto>> ExecuteAsync(int page, int pageSize)
        {
            var result = await _skillRepository.GetPagedSkillsAsync(page, pageSize);
            return new PagedResult<SkillDto>
            (
                _mapper.Map<List<SkillDto>>(result.Items),
                result.TotalItems,
                result.PageSize,
                result.CurrentPage
            );
        }
    }
}

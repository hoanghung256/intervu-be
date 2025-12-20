using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.Skill;

namespace Intervu.Application.Interfaces.UseCases.Skill
{
    public interface IGetAllSkills
    {
        Task<PagedResult<SkillDto>> ExecuteAsync(int page, int pageSize);
    }
}

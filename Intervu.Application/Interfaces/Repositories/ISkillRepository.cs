using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Application.Common;
using Intervu.Domain.Entities;

namespace Intervu.Application.Interfaces.Repositories
{
    public interface ISkillRepository
    {
        Task<PagedResult<Skill>> GetPagedSkillsAsync(int page, int pageSize);
    }
}

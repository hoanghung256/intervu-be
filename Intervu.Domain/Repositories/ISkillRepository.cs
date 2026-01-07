using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Domain.Entities;

namespace Intervu.Domain.Repositories
{
    public interface ISkillRepository
    {
        Task<(IReadOnlyList<Skill> Items, int TotalCount)> GetPagedSkillsAsync(int page, int pageSize);
        Task<IReadOnlyList<Skill>> GetByIdsAsync(IEnumerable<Guid> ids);
    }
}

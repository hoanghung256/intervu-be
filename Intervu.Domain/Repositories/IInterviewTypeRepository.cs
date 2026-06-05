using Intervu.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Domain.Repositories
{
    public interface IInterviewTypeRepository : IRepositoryBase<InterviewType>
    {
        /// <summary>
        /// Paged interview types. When <paramref name="activeOnly"/> is true, only Active statuses (public/coach). When false, all statuses (admin).
        /// </summary>
        Task<(IEnumerable<InterviewType> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, bool activeOnly);

        Task<InterviewType?> GetByNameAsync(string name);
    }
}

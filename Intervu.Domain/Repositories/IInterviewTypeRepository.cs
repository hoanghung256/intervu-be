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
        Task<IEnumerable<InterviewType>> GetList(int page, int pageSize);
    }
}

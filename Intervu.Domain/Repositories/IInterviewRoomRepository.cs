using Intervu.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Domain.Repositories
{
    public interface IInterviewRoomRepository : IRepositoryBase<InterviewRoom>
    {
        Task<IEnumerable<InterviewRoom>> GetListByCandidateId(Guid candidateId);
        Task<IEnumerable<InterviewRoom>> GetListByInterviewerId(Guid interviewerId);
        Task<IEnumerable<InterviewRoom>> GetList();
    }
}

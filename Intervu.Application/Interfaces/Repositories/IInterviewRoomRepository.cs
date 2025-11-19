using Intervu.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.Repositories
{
    public interface IInterviewRoomRepository : IRepositoryBase<InterviewRoom>
    {
        Task<IEnumerable<InterviewRoom>> GetListByIntervieweeId(int intervieweeId);
        Task<IEnumerable<InterviewRoom>> GetListByInterviewerId(int interviewerId);
        Task<IEnumerable<InterviewRoom>> GetList();
    }
}

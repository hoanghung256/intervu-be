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
        Task<IEnumerable<InterviewRoom>> GetListByCoachId(Guid coachId);
        Task<IEnumerable<InterviewRoom>> GetList();
        Task<IEnumerable<InterviewRoom>> GetConflictingRoomsAsync(Guid userId, DateTime startTime, DateTime endTime);
        Task<InterviewRoom?> GetByIdWithDetailsAsync(Guid id);
    }
}

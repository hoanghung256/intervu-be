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
        
        /// <summary>
        /// Get rooms with participant names for a candidate
        /// </summary>
        Task<IEnumerable<(InterviewRoom Room, string? CandidateName, string? CoachName)>> GetListWithNamesByCandidateIdAsync(Guid candidateId);
        
        /// <summary>
        /// Get rooms with participant names for a coach
        /// </summary>
        Task<IEnumerable<(InterviewRoom Room, string? CandidateName, string? CoachName)>> GetListWithNamesByCoachIdAsync(Guid coachId);
    }
}

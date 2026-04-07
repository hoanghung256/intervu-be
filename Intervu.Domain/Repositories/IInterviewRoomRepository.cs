using Intervu.Domain.Entities;
using System;
using System.Collections.Generic;
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
        /// Get rooms with participant summary fields for a candidate.
        /// </summary>
        Task<IEnumerable<(
            InterviewRoom Room,
            string? CandidateName,
            string? CandidateProfilePicture,
            string? CandidateSlugProfileUrl,
            string? CoachName,
            string? CoachProfilePicture,
            string? CoachSlugProfileUrl)>> GetListWithNamesByCandidateIdAsync(Guid candidateId);
        
        /// <summary>
        /// Get rooms with participant summary fields for a coach.
        /// </summary>
        Task<IEnumerable<(
            InterviewRoom Room,
            string? CandidateName,
            string? CandidateProfilePicture,
            string? CandidateSlugProfileUrl,
            string? CoachName,
            string? CoachProfilePicture,
            string? CoachSlugProfileUrl)>> GetListWithNamesByCoachIdAsync(Guid coachId);

        /// <summary>
        /// Get interview rooms linked to a booking request
        /// </summary>
        Task<List<InterviewRoom>> GetByBookingRequestIdAsync(Guid bookingRequestId);
    }
}

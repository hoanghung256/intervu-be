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

        /// <summary>
        /// Count completed interviews for a coach within a date range.
        /// </summary>
        Task<int> GetCompletedCountByCoachIdAsync(Guid coachId, DateTime from, DateTime to);

        /// <summary>
        /// Get upcoming scheduled sessions for a coach with candidate info, ordered by scheduled time.
        /// </summary>
        Task<List<(InterviewRoom Room, string? CandidateName, string? CandidateProfilePicture, string? BookingStatus)>>
            GetUpcomingByCoachIdAsync(Guid coachId, int limit);

        /// <summary>
        /// Get service distribution (InterviewType name → count) for completed rooms of a coach.
        /// </summary>
        Task<List<(string ServiceName, int Count)>> GetServiceDistributionByCoachIdAsync(Guid coachId);
    }
}

using Intervu.Application.DTOs.InterviewRoom;
using Intervu.Domain.Entities.Constants;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.InterviewRoom
{
    public interface IGetSessions
    {
        /// <summary>
        /// Returns a paged list of interview sessions for the given user.
        /// Rooms sharing the same BookingRequestId collapse into one session; standalone rooms
        /// (no BookingRequestId) become single-round sessions keyed by the room's Id.
        /// Includes aggregate stats over ALL user sessions and, for interviewers, a
        /// pending coach-evaluation session when one exists.
        /// </summary>
        Task<PagedSessionsResultDto> ExecuteAsync(UserRole role, Guid userId, GetSessionsRequestDto request);
    }
}

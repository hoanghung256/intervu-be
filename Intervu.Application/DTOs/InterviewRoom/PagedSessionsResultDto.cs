using Intervu.Application.DTOs.Common;

namespace Intervu.Application.DTOs.InterviewRoom
{
    /// <summary>
    /// Response envelope for GET /api/v1/interviewroom/sessions.
    /// Composes a standard paged result with global stats and an optional pending-eval prompt.
    /// </summary>
    public class PagedSessionsResultDto
    {
        public PagedResult<SessionDto> Page { get; set; }
        public SessionStatsDto Stats { get; set; }

        /// <summary>
        /// Populated for interviewer users when they have a completed room with an unfinished evaluation.
        /// FE opens CoachEvaluationModal with this session's active room. Null otherwise.
        /// </summary>
        public SessionDto? PendingCoachEvaluationSession { get; set; }

        public PagedSessionsResultDto(
            PagedResult<SessionDto> page,
            SessionStatsDto stats,
            SessionDto? pendingCoachEvaluationSession)
        {
            Page = page;
            Stats = stats;
            PendingCoachEvaluationSession = pendingCoachEvaluationSession;
        }
    }
}

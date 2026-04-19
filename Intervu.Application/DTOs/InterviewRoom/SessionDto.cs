using System;
using System.Collections.Generic;

namespace Intervu.Application.DTOs.InterviewRoom
{
    /// <summary>
    /// Represents an interview "session": a group of interview rooms sharing the same BookingRequestId.
    /// Inherits every field of the active round (so FE can read room-level fields directly on the session).
    /// Standalone rooms (no BookingRequestId) appear as single-round sessions.
    /// </summary>
    public class SessionDto : InterviewRoomDto
    {
        /// <summary>
        /// Stable identifier: BookingRequestId when set, otherwise the single room's Id.
        /// </summary>
        public Guid SessionId { get; set; }

        /// <summary>
        /// 1-based index of the active round within <see cref="Rounds"/>.
        /// Active round precedence: first OnGoing &gt; first Scheduled &gt; last by ScheduledTime.
        /// </summary>
        public int CurrentRound { get; set; }

        /// <summary>
        /// Number of rounds in this session (1 for standalone rooms).
        /// </summary>
        public int TotalRounds { get; set; }

        /// <summary>
        /// All rooms belonging to this session, sorted ascending by ScheduledTime.
        /// </summary>
        public List<InterviewRoomDto> Rounds { get; set; } = new();
    }
}

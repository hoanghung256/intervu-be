namespace Intervu.Application.DTOs.InterviewRoom
{
    /// <summary>
    /// Aggregate KPIs computed across ALL of the user's sessions (ignoring the Statuses filter).
    /// Identical for both Upcoming and Past tab requests so the UI header stays stable.
    /// </summary>
    public class SessionStatsDto
    {
        /// <summary>Sessions whose derived status is Scheduled or OnGoing.</summary>
        public int Upcoming { get; set; }

        /// <summary>Sessions whose derived status is Completed.</summary>
        public int Completed { get; set; }

        /// <summary>
        /// Role-aware average for completed sessions, rounded to 1 dp.
        /// Candidate: average of coach evaluation Score (scale 10).
        /// Coach: average of candidate feedback Rating (scale 5).
        /// Null when no evaluated rooms exist.
        /// </summary>
        public double? AvgScore { get; set; }

        /// <summary>
        /// Milliseconds until the earliest future Scheduled session. Null when none.
        /// </summary>
        public long? NextSessionInMs { get; set; }
    }
}

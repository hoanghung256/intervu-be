namespace Intervu.Application.DTOs.CoachDashboard
{
    public class CoachUpcomingSessionDto
    {
        public Guid InterviewRoomId { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public string? CandidateProfilePicture { get; set; }
        public string RoomIdDisplay { get; set; } = string.Empty;
        public DateTime? ScheduledTime { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}

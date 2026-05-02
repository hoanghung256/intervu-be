namespace Intervu.Application.DTOs.CoachDashboard
{
    public class CoachPendingRequestDto
    {
        public Guid BookingRequestId { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public string? CandidateProfilePicture { get; set; }
        public string? CandidateJobTitle { get; set; }
        public int? CandidateExperienceYears { get; set; }
        public string? Message { get; set; }
        /// <summary>Optional note from the candidate (camelCase: candidateNote).</summary>
        public string? CandidateNote { get; set; }
        public DateTime RequestedAt { get; set; }
    }
}

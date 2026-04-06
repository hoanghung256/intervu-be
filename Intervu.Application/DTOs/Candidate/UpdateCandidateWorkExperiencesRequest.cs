namespace Intervu.Application.DTOs.Candidate
{
    public class UpdateCandidateWorkExperiencesRequest
    {
        public List<CandidateWorkExperienceDto> WorkExperiences { get; set; } = new();
    }
}

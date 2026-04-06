namespace Intervu.Application.DTOs.Coach
{
    public class UpdateCoachWorkExperiencesRequest
    {
        public List<CoachWorkExperienceDto> WorkExperiences { get; set; } = new();
    }
}

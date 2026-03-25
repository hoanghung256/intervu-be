using Intervu.Application.DTOs.Coach;

namespace Intervu.Application.DTOs.SmartSearch
{
    public class SmartSearchResultDto
    {
        public Guid CoachId { get; set; }
        public double MatchScore { get; set; }
        public CoachViewDto? Coach { get; set; }
    }
}

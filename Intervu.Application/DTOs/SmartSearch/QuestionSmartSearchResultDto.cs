using Intervu.Application.DTOs.Question;

namespace Intervu.Application.DTOs.SmartSearch
{
    public class QuestionSmartSearchResultDto
    {
        public Guid QuestionId { get; set; }
        public double MatchScore { get; set; }
        public QuestionSearchResultDto? Question { get; set; }
    }
}

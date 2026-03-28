using Intervu.Application.DTOs.Coach;

namespace Intervu.Application.DTOs.SmartSearch
{
    public class CompanySummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? LogoPath { get; set; }
    }

    public class SmartSearchResultDto
    {
        public Guid CoachId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
        public string? SlugProfileUrl { get; set; }
        public int ExperienceYears { get; set; }
        public string? PortfolioUrl { get; set; }
        public List<string> TopSkills { get; set; } = new List<string>();
        public List<CompanySummaryDto> Companies { get; set; } = new List<CompanySummaryDto>();

        // Scores
        public double MatchScore { get; set; }
        public double? RerankScore { get; set; }
        public double FinalScore { get; set; } // The definitive score determining the sort order

        // AI Metadata
        public string? Reasoning { get; set; }
        public string? RerankSource { get; set; } // e.g. "Pinecone" or "Gemini"
    }
}

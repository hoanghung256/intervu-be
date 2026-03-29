namespace Intervu.Application.DTOs.SmartSearch
{
    public class QuestionSmartSearchRequestDto
    {
        public string Query { get; set; } = string.Empty;
        public int TopK { get; set; } = 5;
        public string? Namespace { get; set; }
        public string EntityType { get; set; } = "question";
    }
}

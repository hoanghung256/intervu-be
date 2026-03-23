namespace Intervu.Domain.Entities
{
    public class EvaluationItem
    {
        public string Type { get; set; }
        public string Question { get; set; }
    }

    public class EvaluationResult
    {
        public string Type { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public int Score { get; set; } = 0; // per 10
    }
}
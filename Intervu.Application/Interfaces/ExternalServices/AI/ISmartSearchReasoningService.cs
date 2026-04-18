namespace Intervu.Application.Interfaces.ExternalServices.AI
{
    public interface ISmartSearchReasoningService
    {
        /// <summary>
        /// Sends a list of candidates to the LLM to re-evaluate and provide reasoning
        /// based on the user's initial natural language query.
        /// </summary>
        /// <param name="query">The user's original search query.</param>
        /// <param name="candidates">The top K candidates retrieved from the Vector DB.</param>
        /// <returns>A list of reasoning results containing the new score and reasoning text, mapped by candidate ID.</returns>
        Task<List<ReasoningResult>> RerankAndReasonAsync(string query, List<ReasoningCandidate> candidates, string? useCase = null);
    }

    public class ReasoningCandidate
    {
        public string Id { get; set; } = string.Empty;
        
        /// <summary>
        /// Safely formatted and truncated summary of the candidate (Coach or Question)
        /// to send to the LLM. Should not contain huge blocks of untrusted text.
        /// </summary>
        public string Summary { get; set; } = string.Empty;
    }

    public class ReasoningResult
    {
        public string Id { get; set; } = string.Empty;
        public double Score { get; set; }
        public string Reasoning { get; set; } = string.Empty;
    }
}

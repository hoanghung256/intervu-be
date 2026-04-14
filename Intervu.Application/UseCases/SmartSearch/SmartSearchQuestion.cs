using Intervu.Application.DTOs.Question;
using Intervu.Application.DTOs.SmartSearch;
using Intervu.Application.Interfaces.ExternalServices.Pinecone;
using Intervu.Application.Interfaces.UseCases.SmartSearch;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Configuration;

namespace Intervu.Application.UseCases.SmartSearch
{
    public class SmartSearchQuestion : ISmartSearchQuestion
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly IVectorStoreService _vectorStoreService;
        private readonly IQuestionRepository _questionRepository;
        private readonly Intervu.Application.Interfaces.ExternalServices.AI.ISmartSearchReasoningService _reasoningService;
        private readonly string _questionNamespace;

        public SmartSearchQuestion(
            IEmbeddingService embeddingService,
            IVectorStoreService vectorStoreService,
            IQuestionRepository questionRepository,
            Intervu.Application.Interfaces.ExternalServices.AI.ISmartSearchReasoningService reasoningService,
            IConfiguration configuration)
        {
            _embeddingService = embeddingService;
            _vectorStoreService = vectorStoreService;
            _questionRepository = questionRepository;
            _reasoningService = reasoningService;

            var configuredNamespace = configuration["PineCone:PINECONE_QUESTION_NAMESPACE"];
            _questionNamespace = string.IsNullOrWhiteSpace(configuredNamespace)
                ? "questions"
                : configuredNamespace.Trim();
        }

        public async Task<List<QuestionSmartSearchResultDto>> ExecuteAsync(QuestionSmartSearchRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Query))
                throw new ArgumentException("Search query cannot be empty.");

            if (!string.Equals(request.EntityType, "question", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("EntityType must be 'question'.");

            // Use query embedding mode for search text.
            var queryVector = await _embeddingService.GetEmbeddingAsync(request.Query, "query");

            // Limit search to question vectors in the target namespace.
            var vectorMatches = await _vectorStoreService.SearchAsync(
                queryVector,
                request.TopK,
                string.IsNullOrWhiteSpace(request.Namespace) ? _questionNamespace : request.Namespace,
                new Dictionary<string, object> { ["entityType"] = "question" });

            if (!vectorMatches.Any())
                return new List<QuestionSmartSearchResultDto>();

            // Validate metadata before hydrating from DB.
            var validMatches = vectorMatches.Where(IsValidQuestionMatch).ToList();
            if (!validMatches.Any())
                return new List<QuestionSmartSearchResultDto>();

            var orderedIds = validMatches
                .Select(m => Guid.TryParse(m.Id, out var id) ? id : Guid.Empty)
                .Where(id => id != Guid.Empty)
                .ToList();

            if (!orderedIds.Any())
                return new List<QuestionSmartSearchResultDto>();

            var questions = await _questionRepository.GetByIdsAsync(orderedIds);
            var questionsById = questions.ToDictionary(q => q.Id, q => q);

            var results = new List<QuestionSmartSearchResultDto>();
            var reasoningCandidates = new List<Intervu.Application.Interfaces.ExternalServices.AI.ReasoningCandidate>();

            foreach (var match in validMatches)
            {
                if (!Guid.TryParse(match.Id, out var questionId)) continue;
                if (!questionsById.TryGetValue(questionId, out var question)) continue;

                var qstSearchResult = new QuestionSearchResultDto
                {
                    Id = question.Id,
                    Title = question.Title,
                    Content = question.Content,
                    CompanyNames = question.QuestionCompanies?.Select(qc => qc.Company?.Name ?? string.Empty).ToList() ?? new(),
                    Roles = question.QuestionRoles?.Select(qr => qr.Role.ToString()).ToList() ?? new(),
                    Tags = question.QuestionTags?.Select(qt => qt.Tag?.Name ?? string.Empty).ToList() ?? new(),
                    Vote = question.Vote,
                    CommentCount = question.Comments?.Count ?? 0
                };

                results.Add(new QuestionSmartSearchResultDto
                {
                    QuestionId = questionId,
                    MatchScore = match.Score,
                    Question = qstSearchResult,
                    RerankSource = "Pinecone"
                });

                reasoningCandidates.Add(new Intervu.Application.Interfaces.ExternalServices.AI.ReasoningCandidate
                {
                    Id = questionId.ToString(),
                    Summary = $"Title: {question.Title}. Companies: {string.Join(", ", qstSearchResult.CompanyNames)}. Tags: {string.Join(", ", qstSearchResult.Tags)}."
                });
            }

            // Step 4: AI Reasoning & Re-ranking
            if (reasoningCandidates.Any())
            {
                var reasoningResults = await _reasoningService.RerankAndReasonAsync(request.Query, reasoningCandidates);

                if (reasoningResults.Any())
                {
                    // Apply new scores and reasoning
                    var reasoningMap = reasoningResults.ToDictionary(r => r.Id, r => r);
                    foreach (var result in results)
                    {
                        if (reasoningMap.TryGetValue(result.QuestionId.ToString(), out var aiResult))
                        {
                            result.RerankScore = aiResult.Score;
                            result.Reasoning = aiResult.Reasoning;
                            result.RerankSource = "Gemini";
                        }
                        else
                        {
                            result.RerankScore = 0; // Penalize if dropped
                        }
                    }

                    results = results.OrderByDescending(r => r.RerankScore ?? 0).ToList();
                }
                else
                {
                    results = results.OrderByDescending(r => r.MatchScore).ToList();
                }
            }

            return results;
        }

        private static bool IsValidQuestionMatch(VectorMatch match)
        {
            // Guard against cross-entity or mismatched metadata.
            if (match.Metadata == null)
            {
                return false;
            }

            if (!match.Metadata.TryGetValue("entityType", out var entityType) ||
                !string.Equals(entityType, "question", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (match.Metadata.TryGetValue("entityId", out var entityId) &&
                Guid.TryParse(entityId, out var parsedEntityId) &&
                Guid.TryParse(match.Id, out var parsedMatchId))
            {
                return parsedEntityId == parsedMatchId;
            }

            return true;
        }
    }
}

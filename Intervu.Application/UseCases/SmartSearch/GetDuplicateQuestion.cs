using Intervu.Application.DTOs.Question;
using Intervu.Application.DTOs.SmartSearch;
using Intervu.Application.Interfaces.ExternalServices.Pinecone;
using Intervu.Application.Interfaces.UseCases.SmartSearch;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.SmartSearch
{
    public class GetDuplicateQuestion : IGetDuplicateQuestion
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly IVectorStoreService _vectorStoreService;
        private readonly IQuestionRepository _questionRepository;
        private readonly string _questionNamespace;

        public GetDuplicateQuestion(
            IEmbeddingService embeddingService,
            IVectorStoreService vectorStoreService,
            IQuestionRepository questionRepository,
            IConfiguration configuration)
        {
            _embeddingService = embeddingService;
            _vectorStoreService = vectorStoreService;
            _questionRepository = questionRepository;

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
                new Dictionary<string, string> { ["entityType"] = "question" });

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
            foreach (var match in validMatches)
            {
                if (!Guid.TryParse(match.Id, out var questionId))
                    continue;

                if (!questionsById.TryGetValue(questionId, out var question))
                    continue;

                results.Add(new QuestionSmartSearchResultDto
                {
                    QuestionId = questionId,
                    MatchScore = match.Score,
                    Question = new QuestionSearchResultDto
                    {
                        Id = question.Id,
                        Title = question.Title,
                        Content = question.Content,
                        CompanyNames = question.QuestionCompanies?.Select(qc => qc.Company?.Name ?? string.Empty).ToList() ?? new(),
                        Roles = question.QuestionRoles?.Select(qr => qr.Role.ToString()).ToList() ?? new(),
                        Tags = question.QuestionTags?.Select(qt => qt.Tag?.Name ?? string.Empty).ToList() ?? new(),
                        Vote = question.Vote,
                        CommentCount = question.Comments?.Count ?? 0
                    }
                });
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

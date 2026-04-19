using Intervu.Application.Interfaces.ExternalServices.Pinecone;
using Intervu.Application.Interfaces.UseCases.SmartSearch;
using Intervu.Domain.Entities.Constants.QuestionConstants;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using QuestionEntity = Intervu.Domain.Entities.Question;

namespace Intervu.Application.UseCases.SmartSearch
{
    public class SyncQuestionVectors : ISyncQuestionVectors
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly IEmbeddingService _embeddingService;
        private readonly IVectorStoreService _vectorStoreService;
        private readonly string _questionNamespace;

        public SyncQuestionVectors(
            IQuestionRepository questionRepository,
            IEmbeddingService embeddingService,
            IVectorStoreService vectorStoreService,
            IConfiguration configuration)
        {
            _questionRepository = questionRepository;
            _embeddingService = embeddingService;
            _vectorStoreService = vectorStoreService;

            var configuredNamespace = configuration["PineCone:PINECONE_QUESTION_NAMESPACE"];
            _questionNamespace = string.IsNullOrWhiteSpace(configuredNamespace)
                ? "questions"
                : configuredNamespace.Trim();
        }

        public async Task<int> ExecuteAsync()
        {
            const int pageSize = 500;
            var page = 1;
            var syncedCount = 0;

            // Sync by pages to avoid loading all questions at once.
            while (true)
            {
                var (questions, totalCount) = await _questionRepository.GetPagedAsync(
                    searchTerm: null,
                    companyId: null,
                    tagId: null,
                    category: null,
                    role: null,
                    level: null,
                    round: null,
                    sortBy: SortOption.New,
                    page: page,
                    pageSize: pageSize);

                if (!questions.Any())
                {
                    break;
                }

                // Only approved and visible questions are searchable.
                foreach (var question in questions.Where(q => q.Status == QuestionStatus.Approved && !q.IsHidden))
                {
                    var documentText = BuildQuestionDocumentText(question);
                    // Use passage embedding mode for indexed content.
                    var vector = await _embeddingService.GetEmbeddingAsync(documentText, "passage");
                    var metadata = BuildMetadata(question);

                    await _vectorStoreService.UpsertAsync(question.Id.ToString(), vector, metadata, _questionNamespace);
                    syncedCount++;
                }

                if (page * pageSize >= totalCount)
                {
                    break;
                }

                page++;
            }

            return syncedCount;
        }

        /// <summary>
        /// Build text used to generate the question embedding vector.
        /// </summary>
        private static string BuildQuestionDocumentText(QuestionEntity question)
        {
            var parts = new List<string>
            {
                $"Title: {question.Title}",
                $"Content: {question.Content}",
                $"Category: {question.Category}",
                $"Level: {question.Level}",
                $"Round: {question.Round}"
            };

            if (question.QuestionRoles.Any())
            {
                parts.Add($"Roles: {string.Join(", ", question.QuestionRoles.Select(x => x.Role.ToString()))}");
            }

            if (question.QuestionTags.Any())
            {
                parts.Add($"Tags: {string.Join(", ", question.QuestionTags.Select(x => x.Tag?.Name).Where(x => !string.IsNullOrWhiteSpace(x)))}");
            }

            if (question.QuestionCompanies.Any())
            {
                parts.Add($"Companies: {string.Join(", ", question.QuestionCompanies.Select(x => x.Company?.Name).Where(x => !string.IsNullOrWhiteSpace(x)))}");
            }

            return string.Join(". ", parts);
        }

        /// <summary>
        /// Store minimal identity and filterable fields in vector metadata.
        /// </summary>
        private static Dictionary<string, object> BuildMetadata(QuestionEntity question)
        {
            var metadata = new Dictionary<string, object>
            {
                ["entityType"] = "question",
                ["entityId"] = question.Id.ToString(),
                ["questionId"] = question.Id.ToString(),
                ["title"] = question.Title,
                ["category"] = question.Category.ToString(),
                ["level"] = question.Level.ToString(),
                ["round"] = question.Round.ToString()
            };

            return metadata;
        }
    }
}

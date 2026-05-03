using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Intervu.Application.DTOs.Assessment;
using Intervu.Application.Interfaces.Services;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Intervu.Infrastructure.Services
{
    public sealed class RoadmapPracticeEnrichmentService : IRoadmapPracticeEnrichmentService
    {
        private const int SearchLimit = 8;
        private const int QuestionsPerChild = 3;

        private readonly IQuestionRepository _questionRepository;
        private readonly ILogger<RoadmapPracticeEnrichmentService> _logger;

        public RoadmapPracticeEnrichmentService(
            IQuestionRepository questionRepository,
            ILogger<RoadmapPracticeEnrichmentService> logger)
        {
            _questionRepository = questionRepository;
            _logger = logger;
        }

        public async Task EnrichChildSkillQuestionsAsync(SurveyRoadmapDto? roadmap, CancellationToken cancellationToken = default)
        {
            if (roadmap?.Phases == null || roadmap.Phases.Count == 0)
            {
                return;
            }

            var usedQuestionIds = new HashSet<Guid>();
            var keywordCache = new Dictionary<string, List<Question>>(StringComparer.OrdinalIgnoreCase);

            foreach (var phase in roadmap.Phases)
            {
                cancellationToken.ThrowIfCancellationRequested();

                foreach (var node in phase.Nodes ?? Enumerable.Empty<SurveyRoadmapNodeDto>())
                {
                    if (!ShouldEnrichNode(node))
                    {
                        continue;
                    }

                    foreach (var child in node.ChildSkills ?? Enumerable.Empty<SurveyRoadmapChildSkillDto>())
                    {
                        if (child == null)
                        {
                            continue;
                        }

                        if (child.Questions is { Count: > 0 })
                        {
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(child.Name))
                        {
                            continue;
                        }

                        var keyword = BuildSearchKeyword(child.Name, node.SkillName);
                        if (string.IsNullOrWhiteSpace(keyword))
                        {
                            continue;
                        }

                        if (!keywordCache.TryGetValue(keyword, out var matches))
                        {
                            try
                            {
                                matches = await _questionRepository.SearchAsync(keyword, SearchLimit);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Question search failed for roadmap enrichment keyword {Keyword}", keyword);
                                matches = new List<Question>();
                            }

                            keywordCache[keyword] = matches;
                        }

                        var added = 0;
                        foreach (var q in matches)
                        {
                            if (added >= QuestionsPerChild)
                            {
                                break;
                            }

                            if (!usedQuestionIds.Add(q.Id))
                            {
                                continue;
                            }

                            child.Questions.Add(new SurveyRoadmapQuestionDto
                            {
                                Id = q.Id.ToString(),
                                Title = q.Title ?? string.Empty,
                                Difficulty = q.Level.ToString(),
                            });
                            added++;
                        }
                    }
                }
            }
        }

        private static bool ShouldEnrichNode(SurveyRoadmapNodeDto node)
        {
            var p = node.PillarType?.Trim() ?? string.Empty;
            return p.Equals("HARD_SKILL", StringComparison.OrdinalIgnoreCase)
                   || p.Equals("SOFT_SKILL", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Prefer a substantive token from the child label, then from the parent skill name, for SearchAsync contains matching.
        /// </summary>
        private static string BuildSearchKeyword(string childName, string skillName)
        {
            static string FirstToken(string text, int minLength)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    return string.Empty;
                }

                foreach (var part in text.Split(
                             new[] { ' ', ',', ';', '/', '-', ':', '&' },
                             StringSplitOptions.RemoveEmptyEntries))
                {
                    var t = part.Trim();
                    if (t.Length >= minLength)
                    {
                        return t;
                    }
                }

                return string.Empty;
            }

            var fromChild = FirstToken(childName, 3);
            if (!string.IsNullOrEmpty(fromChild))
            {
                return fromChild;
            }

            fromChild = FirstToken(childName, 2);
            if (!string.IsNullOrEmpty(fromChild))
            {
                return fromChild;
            }

            var fromSkill = FirstToken(skillName, 3);
            if (!string.IsNullOrEmpty(fromSkill))
            {
                return fromSkill;
            }

            var trimmed = childName.Trim();
            if (trimmed.Length >= 3)
            {
                return trimmed.Length <= 24 ? trimmed : trimmed[..24];
            }

            trimmed = (skillName ?? string.Empty).Trim();
            if (trimmed.Length >= 3)
            {
                return trimmed.Length <= 24 ? trimmed : trimmed[..24];
            }

            return string.Empty;
        }
    }
}

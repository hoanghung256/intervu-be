using System;
using System.Text.Json;
using System.Threading.Tasks;
using Intervu.Application.DTOs.Assessment;
using Intervu.Application.Interfaces.UseCases.Assessment;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.Assessment
{
    public class SaveAssessmentAnswersUseCase : ISaveAssessmentAnswersUseCase
    {
        private readonly IUserSkillAssessmentSnapshotRepository _snapshotRepository;

        public SaveAssessmentAnswersUseCase(IUserSkillAssessmentSnapshotRepository snapshotRepository)
        {
            _snapshotRepository = snapshotRepository;
        }

        public async Task<SaveAssessmentAnswersResultDto> ExecuteAsync(SaveAssessmentAnswersRequestDto request)
        {
            var userId = request.UserId != Guid.Empty
                ? request.UserId
                : request.ProcessingPayload?.UserId ?? Guid.Empty;

            if (userId == Guid.Empty)
            {
                throw new InvalidOperationException("UserId is required.");
            }

            var answerJson = JsonSerializer.Serialize(request);

            if (request.ProcessingPayload != null)
            {
                var payload = request.ProcessingPayload;
                var snapshot = new UserSkillAssessmentSnapshot
                {
                    UserId = userId,
                    AnswerJson = answerJson,
                    Target = new Target
                    {
                        Roles = payload.Target?.Roles ?? new(),
                        Level = payload.Target?.Level ?? string.Empty,
                        SkillsTarget = payload.Target?.SkillsTarget ?? new(),
                    },
                    Current = new Current
                    {
                        Skills = payload.Current?.Skills?.ConvertAll(s => new SkillLevel
                        {
                            Skill = s.Skill,
                            Level = s.Level,
                            SfiaLevel = s.SfiaLevel ?? 0
                        }) ?? new()
                    },
                    Gap = new Gap
                    {
                        Missing = payload.Gap?.Missing ?? new(),
                        Weak = payload.Gap?.Weak ?? new(),
                    },
                };

                await _snapshotRepository.UpsertSnapshotAsync(snapshot);
            }
            else
            {
                await _snapshotRepository.SaveAnswerJsonAsync(userId, answerJson);
            }

            return new SaveAssessmentAnswersResultDto
            {
                UserId = userId,
                SavedAtUtc = DateTime.UtcNow
            };
        }

        public async Task<SaveAssessmentAnswersResultDto> SaveRawEvaluationAsync(Guid userId, string rawEvaluationJson)
        {
            if (userId == Guid.Empty)
            {
                throw new InvalidOperationException("UserId is required.");
            }

            var snapshot = BuildSnapshotFromRawEvaluation(userId, rawEvaluationJson);
            await _snapshotRepository.UpsertSnapshotAsync(snapshot);

            return new SaveAssessmentAnswersResultDto
            {
                UserId = userId,
                SavedAtUtc = DateTime.UtcNow
            };
        }

        private static UserSkillAssessmentSnapshot BuildSnapshotFromRawEvaluation(Guid userId, string rawEvaluationJson)
        {
            var normalizedRaw = NormalizeJsonPayload(rawEvaluationJson);

            try
            {
                using var document = JsonDocument.Parse(normalizedRaw);
                var root = document.RootElement;

                if (root.ValueKind != JsonValueKind.Object)
                {
                    return new UserSkillAssessmentSnapshot
                    {
                        UserId = userId,
                        AnswerJson = normalizedRaw
                    };
                }

                var answerJson = ExtractObjectJson(root, "answer", "answerJson");
                var targetJson = ExtractObjectJson(root, "target", "targetJson");
                var currentJson = ExtractObjectJson(root, "current", "currentJson");
                var gapJson = ExtractObjectJson(root, "gapJson", "gap", "gap_json");
                var roadmapJson = ExtractObjectJson(root, "roadmap", "roadMap", "roadMapJson");

                return new UserSkillAssessmentSnapshot
                {
                    UserId = userId,
                    AnswerJson = answerJson,
                    TargetJson = targetJson,
                    CurrentJson = currentJson,
                    GapJson = gapJson,
                    RoadMapJson = roadmapJson
                };
            }
            catch (JsonException)
            {
                return new UserSkillAssessmentSnapshot
                {
                    UserId = userId,
                    AnswerJson = normalizedRaw
                };
            }
        }

        private static string ExtractObjectJson(JsonElement root, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                if (!TryGetPropertyIgnoreCase(root, propertyName, out var propertyValue))
                {
                    continue;
                }

                if (propertyValue.ValueKind == JsonValueKind.Null || propertyValue.ValueKind == JsonValueKind.Undefined)
                {
                    return "{}";
                }

                return propertyValue.GetRawText();
            }

            return "{}";
        }

        private static bool TryGetPropertyIgnoreCase(JsonElement element, string propertyName, out JsonElement value)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        private static string NormalizeJsonPayload(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return "{}";
            }

            var trimmed = json.Trim();
            return string.Equals(trimmed, "null", StringComparison.OrdinalIgnoreCase)
                ? "{}"
                : trimmed;
        }
    }
}

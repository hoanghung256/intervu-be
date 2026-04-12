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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.DTOs.Assessment;
using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.Services;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;

namespace Intervu.Application.Services
{
    public class AssessmentService : IAssessmentService
    {
        private readonly IUserSkillAssessmentSnapshotRepository _snapshotRepository;
        private readonly IAiService _aiService;

        public AssessmentService(
            IUserSkillAssessmentSnapshotRepository snapshotRepository,
            IAiService aiService)
        {
            _snapshotRepository = snapshotRepository;
            _aiService = aiService;
        }

        private static object? DeserializeJson(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<object>(json);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        public async Task<SurveySummaryResultDto> ProcessSurveyResponsesAsync(SurveyResponsesDto request)
        {
            int MapLevel(string lvl) => lvl?.ToLowerInvariant() switch
            {
                "none" => 0,
                "basic" => 1,
                "intermediate" => 2,
                "advanced" => 3,
                "beginner" => 0,
                "comfortable" => 1,
                "confident" => 2,
                "expert" => 3,
                _ => 0
            };

            var responses = request.Responses ?? new List<ResponseItem>();

            var byPhase = responses
                .GroupBy(r => r.Phase ?? string.Empty)
                .ToDictionary(g => g.Key, g => g.ToList());

            var summary = new Dictionary<string, object>();

            foreach (var kv in byPhase)
            {
                var phase = kv.Key;
                var items = kv.Value;
                var scores = items.Select(i => MapLevel(i.SelectedLevel)).ToList();
                var avg = scores.Any() ? Math.Round(scores.Average(), 2) : 0;
                var toImprove = items.Where(i => MapLevel(i.SelectedLevel) <= 1).Select(i => i.Skill).ToList();

                summary[phase] = new
                {
                    AverageScore = avg,
                    Questions = items.Select(i => new { i.Skill, i.SelectedLevel }).ToList(),
                    NeedsImprovement = toImprove
                };
            }

            var lines = new List<string>();
            foreach (var kv in summary)
            {
                var obj = (dynamic)kv.Value;
                var avg = obj.AverageScore;
                var level = avg switch
                {
                    <= 0.5 => "None",
                    <= 1.5 => "Basic",
                    <= 2.5 => "Intermediate",
                    _ => "Advanced"
                };

                var needs = ((IEnumerable<object>)obj.NeedsImprovement).Cast<string>().ToList();
                lines.Add($"{kv.Key}: average level {level}. Consider improving: {string.Join(", ", needs)}");
            }

            var summaryText = string.Join("\n", lines);


            var target = new Target
            {
                Roles = request.Target?.Roles ?? new List<string>(),
                Level = request.Target?.Level ?? string.Empty,
                SkillsTarget = request.Target?.SkillsTarget ?? new List<string>()
            };

            var currentSkills = request.Current?.Skills?.Any() == true
                ? request.Current.Skills.Select(s => new SkillLevel
                {
                    Skill = s.Skill,
                    Level = s.Level,
                    SfiaLevel = s.SfiaLevel ?? MapLevel(s.Level)
                }).ToList()
                : responses.Select(r => new SkillLevel
                {
                    Skill = r.Skill,
                    Level = r.SelectedLevel,
                    SfiaLevel = MapLevel(r.SelectedLevel)
                }).ToList();

            var current = new Current
            {
                Skills = currentSkills
            };

            var missing = request.Gap?.Missing?.Any() == true
                ? request.Gap.Missing
                : currentSkills
                .Where(s => s.SfiaLevel == 0)
                .Select(s => s.Skill)
                .ToList();

            var weak = request.Gap?.Weak?.Any() == true
                ? request.Gap.Weak
                : currentSkills
                .Where(s => s.SfiaLevel == 1)
                .Select(s => s.Skill)
                .ToList();

            var gap = new Gap
            {
                Missing = missing,
                Weak = weak
            };

            var snapshot = new UserSkillAssessmentSnapshot
            {
                UserId = request.UserId,
                Target = target,
                Current = current,
                Gap = gap,
                Roadmap = MapRoadmap(request.Roadmap),
                AnswerJson = request.Answer == null
                    ? null
                    : JsonSerializer.Serialize(request.Answer),
            };

            if (request.UserId != Guid.Empty)
            {
                await _snapshotRepository.UpsertSnapshotAsync(snapshot);
            }

            return new SurveySummaryResultDto
            {
                UserId = request.UserId,
                SummaryText = summaryText,
                SummaryObject = summary
            };
        }

        public async Task<UserSkillAssessmentSnapshotDto?> GetUserSkillAssessmentSnapshotAsync(Guid userId)
        {
            var userSkillAssessment = await _snapshotRepository.GetUserSkillAssessmentById(userId);

            if (userSkillAssessment == null) return null;

            return new UserSkillAssessmentSnapshotDto
            {
                UserId = userSkillAssessment.UserId,
                Target = JsonSerializer.Serialize(userSkillAssessment.Target),
                Current = JsonSerializer.Serialize(userSkillAssessment.Current),
                Gap = JsonSerializer.Serialize(userSkillAssessment.Gap),
                AnswerJson = userSkillAssessment.AnswerJson
            };
        }
        
        public async Task<GenerateRoadmapResultDto> GenerateRoadmapFromSurveyAsync(Guid userId, bool forceRegenerate = false)
        {
            if (userId == Guid.Empty)
            {
                throw new InvalidOperationException("UserId is required.");
            }

            var snapshot = await _snapshotRepository.GetUserSkillAssessmentById(userId);
            if (snapshot == null)
            {
                throw new InvalidOperationException("No survey snapshot found for this user.");
            }

            if (!forceRegenerate && snapshot.Roadmap?.Phases?.Any() == true)
            {
                return new GenerateRoadmapResultDto
                {
                    Status = "success",
                    Roadmap = MapRoadmapToSurveyDto(snapshot.Roadmap)
                };
            }

            var target = snapshot.Target;
            var current = snapshot.Current;
            var gap = snapshot.Gap;

            if (target == null || current == null || gap == null)
            {
                throw new InvalidOperationException("Snapshot is incomplete. Target, Current and Gap are required before generating roadmap.");
            }

            var roadmapRequest = new AiGenerateRoadmapRequestDto
            {
                TargetSkill = new AiTargetSkillDto
                {
                    Level = target.Level,
                    Roles = target.Roles ?? new List<string>(),
                    SkillsTarget = target.SkillsTarget ?? new List<string>(),
                },
                CurrentLevel = new AiCurrentLevelDto
                {
                    Skills = (current.Skills ?? new List<SkillLevel>())
                        .Select(skill => new AiSkillLevelDto
                        {
                            Skill = skill.Skill,
                            Level = skill.Level,
                            SfiaLevel = skill.SfiaLevel,
                        })
                        .ToList(),
                },
                Gap = new AiGapDto
                {
                    Weak = gap.Weak ?? new List<string>(),
                    Missing = gap.Missing ?? new List<string>(),
                },
            };

            var aiResponse = await _aiService.GenerateRoadmapAsync(roadmapRequest);
            if (aiResponse == null)
            {
                return new GenerateRoadmapResultDto
                {
                    Status = "failed",
                    Error = "AI roadmap service is unavailable."
                };
            }

            if (!string.Equals(aiResponse.Status, "success", StringComparison.OrdinalIgnoreCase) || aiResponse.Roadmap == null)
            {
                return new GenerateRoadmapResultDto
                {
                    Status = "failed",
                    Error = aiResponse.Error ?? "Failed to generate roadmap."
                };
            }

            snapshot.Roadmap = MapRoadmap(aiResponse.Roadmap);
            await _snapshotRepository.UpsertSnapshotAsync(snapshot);

            return new GenerateRoadmapResultDto
            {
                Status = "success",
                Roadmap = aiResponse.Roadmap
            };
        }

        public async Task<SurveyRoadmapDto?> GetRoadmapByUserIdAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return null;
            }

            var snapshot = await _snapshotRepository.GetUserSkillAssessmentById(userId);
            if (snapshot?.Roadmap == null)
            {
                return null;
            }

            return MapRoadmapToSurveyDto(snapshot.Roadmap);
        }

        private static RoadmapSnapshot? MapRoadmap(SurveyRoadmapDto? roadmap)
        {
            if (roadmap == null)
            {
                return null;
            }

            return new RoadmapSnapshot
            {
                RoadmapMetadata = new RoadmapMetadataSnapshot
                {
                    TargetRole = roadmap.RoadmapMetadata?.TargetRole ?? string.Empty,
                    TargetLevel = roadmap.RoadmapMetadata?.TargetLevel ?? string.Empty,
                    TotalPhases = roadmap.RoadmapMetadata?.TotalPhases ?? 0,
                },
                Phases = (roadmap.Phases ?? new List<SurveyRoadmapPhaseDto>())
                    .Select(phase => new RoadmapPhaseSnapshot
                    {
                        PhaseId = phase.PhaseId,
                        PhaseName = phase.PhaseName,
                        RecommendedCoaches = (phase.RecommendedCoaches ?? new List<SurveyRoadmapCoachDto>())
                            .Select(coach => new RoadmapCoachSnapshot
                            {
                                Id = coach.Id,
                                Name = coach.Name,
                                Role = coach.Role,
                                Rating = coach.Rating,
                                Avatar = coach.Avatar,
                            })
                            .ToList(),
                        MockHistory = (phase.MockHistory ?? new List<SurveyRoadmapMockHistoryDto>())
                            .Select(mock => new RoadmapMockHistorySnapshot
                            {
                                MockId = mock.MockId,
                                MockTitle = mock.MockTitle,
                                InterviewType = mock.InterviewType,
                                CoachName = mock.CoachName,
                                InterviewedAt = mock.InterviewedAt,
                                Evaluation = (mock.Evaluation ?? new List<SurveyRoadmapEvaluationDto>())
                                    .Select(item => new RoadmapEvaluationSnapshot
                                    {
                                        Type = item.Type,
                                        Score = item.Score,
                                        Answer = item.Answer,
                                        Question = item.Question,
                                    })
                                    .ToList(),
                            })
                            .ToList(),
                        Nodes = (phase.Nodes ?? new List<SurveyRoadmapNodeDto>())
                            .Select(node => new RoadmapNodeSnapshot
                            {
                                SkillId = node.SkillId,
                                SkillName = node.SkillName,
                                Assessment = new RoadmapNodeAssessmentSnapshot
                                {
                                    CurrentLevel = node.Assessment?.CurrentLevel ?? string.Empty,
                                    TargetLevel = node.Assessment?.TargetLevel ?? string.Empty,
                                    SfiaLevel = node.Assessment?.SfiaLevel ?? 0,
                                    Status = node.Assessment?.Status ?? string.Empty,
                                    Progress = node.Assessment?.Progress ?? 0,
                                },
                                ChildSkills = (node.ChildSkills ?? new List<SurveyRoadmapChildSkillDto>())
                                    .Select(child => new RoadmapChildSkillSnapshot
                                    {
                                        Name = child.Name,
                                        Questions = (child.Questions ?? new List<SurveyRoadmapQuestionDto>())
                                            .Select(question => new RoadmapQuestionSnapshot
                                            {
                                                Id = question.Id,
                                                Title = question.Title,
                                                Difficulty = question.Difficulty,
                                            })
                                            .ToList(),
                                    })
                                    .ToList(),
                            })
                            .ToList(),
                    })
                    .ToList(),
            };
        }

        private static SurveyRoadmapDto? MapRoadmapToSurveyDto(RoadmapSnapshot? roadmap)
        {
            if (roadmap == null)
            {
                return null;
            }

            return new SurveyRoadmapDto
            {
                RoadmapMetadata = new SurveyRoadmapMetadataDto
                {
                    TargetRole = roadmap.RoadmapMetadata?.TargetRole ?? string.Empty,
                    TargetLevel = roadmap.RoadmapMetadata?.TargetLevel ?? string.Empty,
                    TotalPhases = roadmap.RoadmapMetadata?.TotalPhases ?? 0,
                },
                Phases = (roadmap.Phases ?? new List<RoadmapPhaseSnapshot>())
                    .Select(phase => new SurveyRoadmapPhaseDto
                    {
                        PhaseId = phase.PhaseId,
                        PhaseName = phase.PhaseName,
                        RecommendedCoaches = (phase.RecommendedCoaches ?? new List<RoadmapCoachSnapshot>())
                            .Select(coach => new SurveyRoadmapCoachDto
                            {
                                Id = coach.Id,
                                Name = coach.Name,
                                Role = coach.Role,
                                Rating = coach.Rating,
                                Avatar = coach.Avatar,
                            })
                            .ToList(),
                        MockHistory = (phase.MockHistory ?? new List<RoadmapMockHistorySnapshot>())
                            .Select(mock => new SurveyRoadmapMockHistoryDto
                            {
                                MockId = mock.MockId,
                                MockTitle = mock.MockTitle,
                                InterviewType = mock.InterviewType,
                                CoachName = mock.CoachName,
                                InterviewedAt = mock.InterviewedAt,
                                Evaluation = (mock.Evaluation ?? new List<RoadmapEvaluationSnapshot>())
                                    .Select(item => new SurveyRoadmapEvaluationDto
                                    {
                                        Type = item.Type,
                                        Score = item.Score,
                                        Answer = item.Answer,
                                        Question = item.Question,
                                    })
                                    .ToList(),
                            })
                            .ToList(),
                        Nodes = (phase.Nodes ?? new List<RoadmapNodeSnapshot>())
                            .Select(node => new SurveyRoadmapNodeDto
                            {
                                SkillId = node.SkillId,
                                SkillName = node.SkillName,
                                Assessment = new SurveyRoadmapNodeAssessmentDto
                                {
                                    CurrentLevel = node.Assessment?.CurrentLevel ?? string.Empty,
                                    TargetLevel = node.Assessment?.TargetLevel ?? string.Empty,
                                    SfiaLevel = node.Assessment?.SfiaLevel ?? 0,
                                    Status = node.Assessment?.Status ?? string.Empty,
                                    Progress = node.Assessment?.Progress ?? 0,
                                },
                                ChildSkills = (node.ChildSkills ?? new List<RoadmapChildSkillSnapshot>())
                                    .Select(child => new SurveyRoadmapChildSkillDto
                                    {
                                        Name = child.Name,
                                        Questions = (child.Questions ?? new List<RoadmapQuestionSnapshot>())
                                            .Select(question => new SurveyRoadmapQuestionDto
                                            {
                                                Id = question.Id,
                                                Title = question.Title,
                                                Difficulty = question.Difficulty,
                                            })
                                            .ToList(),
                                    })
                                    .ToList(),
                            })
                            .ToList(),
                    })
                    .ToList(),
            };
        }
    }
}

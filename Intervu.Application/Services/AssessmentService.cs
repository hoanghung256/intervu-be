using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.DTOs.Assessment;
using Intervu.Application.Interfaces.Services;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Intervu.Application.Services
{
    public class AssessmentService : IAssessmentService
    {
        private sealed class EvaluatedResponseItem
        {
            public string QuestionId { get; set; } = string.Empty;
            public string Question { get; set; } = string.Empty;
            public string Phase { get; set; } = string.Empty;
            public string Skill { get; set; } = string.Empty;
            public string Answer { get; set; } = string.Empty;
            public string SelectedLevel { get; set; } = string.Empty;
            public decimal Score { get; set; }
            public bool IsMissing { get; set; }
            public int EffectiveLevel { get; set; }
        }

        private readonly IUserSkillAssessmentSnapshotRepository _snapshotRepository;
        private readonly IInterviewRoomRepository _roomRepository;
        private readonly IAiService _aiService;
        private readonly ILogger<AssessmentService> _logger;
        private static readonly string[] BackendFrameworkSkills =
        {
            "REST API Development",
            "Database Design",
            "ORM and Data Access",
            "Authentication and Authorization",
            "Caching",
            "Message Queue Processing",
            "Background Job Development",
            "System Integration",
            "Microservices Architecture",
            "Performance Optimization",
            "Logging and Monitoring",
            "Automated Backend Testing",
            "CI/CD for Backend Services",
            "Containerization and Deployment",
            "Secure Coding for Backend",
            "Concurrency and Scalability"
        };

        private static readonly string[] FrontendFrameworkSkills =
        {
            "HTML and Semantic Markup",
            "CSS and Styling Architecture",
            "JavaScript and TypeScript Development",
            "Frontend Framework Development",
            "State Management",
            "Responsive UI Development",
            "Web Accessibility",
            "API Integration in Frontend",
            "Frontend Testing",
            "Frontend Performance Optimization",
            "Build Tools and Bundling",
            "UI Component Design",
            "Browser Debugging and Troubleshooting"
        };

        public AssessmentService(
            IUserSkillAssessmentSnapshotRepository snapshotRepository,
            IInterviewRoomRepository roomRepository,
            IAiService aiService,
            ILogger<AssessmentService> logger)
        {
            _snapshotRepository = snapshotRepository;
            _roomRepository = roomRepository;
            _aiService = aiService;
            _logger = logger;
        }


        private static int ParseLevel(string? rawLevel)
        {
            var normalized = (rawLevel ?? string.Empty).Trim().ToLowerInvariant();
            if (normalized is "0" or "1" or "2" or "3" or "4")
            {
                return int.Parse(normalized);
            }

            return normalized switch
            {
                "none" => 0,
                "basic" => 1,
                "beginner" => 1,
                "intermediate" => 2,
                "comfortable" => 2,
                "advanced" => 3,
                "confident" => 3,
                "expert" => 4,
                "lead" => 4,
                "principal" => 4,
                "senior" => 4,
                _ => 0
            };
        }

        private static int MapToSfia(int level)
        {
            return level switch
            {
                <= 0 => 0,
                1 => 2,
                2 => 3,
                3 => 5,
                _ => 6
            };
        }

        private static string MapOverallLevel(double averageLevel)
        {
            return averageLevel switch
            {
                < 0.5 => "None",
                < 1.5 => "Basic",
                < 2.5 => "Intermediate",
                < 3.5 => "Advanced",
                _ => "Expert"
            };
        }

        private static List<string> ResolveSkillScope(SurveyAnswerProfileDto profile, IReadOnlyCollection<SurveyAnswerResponseDto> responses)
        {
            var role = profile.Role?.ToLowerInvariant() ?? string.Empty;
            var baseSkills = role.Contains("front", StringComparison.OrdinalIgnoreCase)
                ? FrontendFrameworkSkills.ToList()
                : role.Contains("full", StringComparison.OrdinalIgnoreCase)
                    ? BackendFrameworkSkills.Concat(FrontendFrameworkSkills).ToList()
                    : BackendFrameworkSkills.ToList();

            var level = profile.Level?.ToLowerInvariant() ?? string.Empty;
            var scopedCount = level switch
            {
                "intern" or "fresher" or "junior" => Math.Min(6, baseSkills.Count),
                "middle" or "mid" => Math.Min(10, baseSkills.Count),
                "senior" => Math.Min(14, baseSkills.Count),
                _ => baseSkills.Count
            };

            var scoped = baseSkills.Take(scopedCount).ToList();
            foreach (var skill in responses
                         .Select(response => response.Skill?.Trim())
                         .Where(skill => !string.IsNullOrWhiteSpace(skill))
                         .Cast<string>())
            {
                if (!scoped.Contains(skill, StringComparer.OrdinalIgnoreCase))
                {
                    scoped.Add(skill);
                }
            }

            return scoped;
        }

        public async Task<SurveySummaryResultDto> ProcessSurveyResponsesAsync(SurveyResponsesDto request, CancellationToken cancellationToken = default)
        {
            return await EvaluateAnswerJsonAsync(
                request.Answer ?? new SurveyAnswerJsonDto(),
                request.Target,
                request.UserId == Guid.Empty ? null : request.UserId,
                cancellationToken);
        }

        public async Task<SurveySummaryResultDto> EvaluateAnswerJsonAsync(
            SurveyAnswerJsonDto answer,
            SurveyTargetDto? target = null,
            Guid? userId = null,
            CancellationToken cancellationToken = default)
        {
            var responses = answer.Responses ?? new List<SurveyAnswerResponseDto>();
            var skillScope = ResolveSkillScope(answer.Profile, responses);
            var evaluatedResponses = new List<EvaluatedResponseItem>();

            foreach (var response in responses)
            {
                var selectedLevel = ParseLevel(response.SelectedLevel);
                var effectiveLevel = response.IsMissing ? 0 : selectedLevel;
                var skill = response.Skill?.Trim() ?? string.Empty;

                evaluatedResponses.Add(new EvaluatedResponseItem
                {
                    QuestionId = response.QuestionId,
                    Question = response.Question,
                    Phase = response.Phase,
                    Skill = skill,
                    Answer = response.Answer,
                    SelectedLevel = response.SelectedLevel,
                    Score = response.Score,
                    IsMissing = response.IsMissing,
                    EffectiveLevel = effectiveLevel
                });
            }

            var currentSkills = skillScope
                .Select(skill =>
                {
                    var matched = evaluatedResponses
                        .Where(item => string.Equals(item.Skill, skill, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    var bestLevel = matched.Any() ? matched.Max(item => item.EffectiveLevel) : 0;
                    var bestScore = matched.Any() ? matched.Max(item => item.Score) : 0m;

                    return new SurveyCurrentSkillDto
                    {
                        Skill = skill,
                        Level = bestLevel.ToString(),
                        Score = (int)Math.Round(bestScore)
                    };
                })
                .ToList();

            var missing = currentSkills
                .Where(skill => skill.Level == "0")
                .Select(skill => skill.Skill)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var averageLevel = evaluatedResponses.Any()
                ? evaluatedResponses.Average(item => item.EffectiveLevel)
                : 0.0;
            var overallLevel = MapOverallLevel(averageLevel);

            var summaryText = missing.Count > 0
                ? $"Assessed {responses.Count} responses for {answer.Profile.Role}. Overall level is {overallLevel}. Missing skills: {string.Join(", ", missing)}."
                : $"Assessed {responses.Count} responses for {answer.Profile.Role}. Overall level is {overallLevel}.";

            var evaluatedAnswerJson = new
            {
                profile = answer.Profile,
                responses = responses
            };

            var normalizedTarget = target ?? new SurveyTargetDto
            {
                Roles = string.IsNullOrWhiteSpace(answer.Profile.Role)
                    ? new List<string>()
                    : new List<string> { answer.Profile.Role },
                Level = answer.Profile.Level ?? string.Empty,
                SkillsTarget = responses
                    .Select(r => r.Skill?.Trim())
                    .Where(skill => !string.IsNullOrWhiteSpace(skill))
                    .Cast<string>()
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList()
            };

            var snapshotTarget = new Target
            {
                Roles = normalizedTarget.Roles ?? new List<string>(),
                Level = normalizedTarget.Level ?? string.Empty,
                SkillsTarget = normalizedTarget.SkillsTarget ?? new List<string>()
            };
            var snapshotCurrent = new Current
            {
                Skills = currentSkills
                    .Select(skill => new SkillLevel
                    {
                        Skill = skill.Skill,
                        Level = skill.Level
                    })
                    .ToList()
            };

            var snapshotGap = new Gap
            {
                Missing = missing
            };

            if (userId.HasValue && userId.Value != Guid.Empty)
            {
                var snapshot = new UserSkillAssessmentSnapshot
                {
                    UserId = userId.Value,
                    Target = snapshotTarget,
                    Current = snapshotCurrent,
                    Gap = snapshotGap,
                    AnswerJson = JsonSerializer.Serialize(evaluatedAnswerJson)
                };
                await _snapshotRepository.UpsertSnapshotAsync(snapshot, cancellationToken);
            }

            return new SurveySummaryResultDto
            {
                UserId = userId,
                SummaryText = summaryText,
                Answer = evaluatedAnswerJson,
                Target = normalizedTarget,
                Current = new SurveyCurrentResultDto
                {
                    Skills = currentSkills
                },
                Missing = missing
            };
        }

        public async Task<UserSkillAssessmentSnapshotDto?> GetUserSkillAssessmentSnapshotAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var userSkillAssessment = await _snapshotRepository.GetUserSkillAssessmentById(userId, cancellationToken);

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
        
        public async Task<GenerateRoadmapResultDto> GenerateRoadmapFromSurveyAsync(Guid userId, bool forceRegenerate = false, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
            {
                throw new InvalidOperationException("UserId is required.");
            }

            var snapshot = await _snapshotRepository.GetUserSkillAssessmentById(userId, cancellationToken);
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

            var aiResponse = await _aiService.GenerateRoadmapAsync(roadmapRequest, cancellationToken, useCase: "GenerateRoadmap");
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
            await _snapshotRepository.UpsertSnapshotAsync(snapshot, cancellationToken);

            return new GenerateRoadmapResultDto
            {
                Status = "success",
                Roadmap = aiResponse.Roadmap
            };
        }

        public async Task<SurveyRoadmapDto?> GetRoadmapByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
            {
                _logger.LogDebug("GetRoadmapByUserId called with empty userId; returning null");
                return null;
            }

            var snapshot = await _snapshotRepository.GetUserSkillAssessmentById(userId, cancellationToken);
            if (snapshot == null)
            {
                _logger.LogDebug("GetRoadmapByUserId: no snapshot found for user {UserId}", userId);
                return null;
            }

            if (snapshot.Roadmap == null)
            {
                _logger.LogDebug("GetRoadmapByUserId: snapshot exists but roadmap is null for user {UserId}", userId);
                return null;
            }

            return MapRoadmapToSurveyDto(snapshot.Roadmap);
        }

        public async Task UpdateRoadmapAfterInterviewAsync(Guid candidateId, Guid interviewRoomId, string coachName, CancellationToken cancellationToken = default)
        {
            if (candidateId == Guid.Empty || interviewRoomId == Guid.Empty)
            {
                return;
            }

            var snapshot = await _snapshotRepository.GetUserSkillAssessmentById(candidateId, cancellationToken);
            if (snapshot?.Roadmap == null || !snapshot.Roadmap.Phases.Any())
            {
                _logger.LogWarning("UpdateRoadmapAfterInterview: no roadmap snapshot found for candidate {CandidateId}", candidateId);
                return;
            }

            var room = await _roomRepository.GetByIdWithDetailsAsync(interviewRoomId);
            if (room == null || room.EvaluationResults == null || !room.EvaluationResults.Any())
            {
                _logger.LogWarning("UpdateRoadmapAfterInterview: room {RoomId} not found or has no evaluation", interviewRoomId);
                return;
            }

            // Resolve interview type name and aim level
            var interviewTypeName = room.CoachInterviewService?.InterviewType?.Name ?? "General";
            var aimLevel = room.AimLevel?.ToString() ?? string.Empty;

            // Build mock history entry
            var mockEntry = new SurveyRoadmapMockHistoryDto
            {
                MockId = interviewRoomId.ToString(),
                MockTitle = $"{interviewTypeName} Interview",
                InterviewType = interviewTypeName,
                CoachName = coachName,
                InterviewedAt = (room.ScheduledTime ?? DateTime.UtcNow).ToString("o"),
                Evaluation = room.EvaluationResults.Select(e => new SurveyRoadmapEvaluationDto
                {
                    Type = e.Type,
                    Score = e.Score,
                    Question = e.Question,
                    Answer = e.Answer
                }).ToList()
            };

            // Append mock history to the first phase that still has incomplete nodes (the active phase)
            var currentRoadmap = MapRoadmapToSurveyDto(snapshot.Roadmap)!;
            var activePhase = currentRoadmap.Phases
                .FirstOrDefault(p => p.Nodes.Any(n => n.Assessment.Status != "Complete"))
                ?? currentRoadmap.Phases.Last();

            // Avoid duplicates: skip if this room was already recorded
            if (!activePhase.MockHistory.Any(m => m.MockId == mockEntry.MockId))
            {
                activePhase.MockHistory.Add(mockEntry);
            }

            // Ask AI to recalculate node progress based on evaluation scores
            var aiRequest = new AiUpdateRoadmapProgressRequestDto
            {
                CurrentRoadmap = currentRoadmap,
                InterviewType = interviewTypeName,
                AimLevel = aimLevel,
                Evaluation = room.EvaluationResults.Select(e => new AiEvaluationItemDto
                {
                    Type = e.Type,
                    Score = e.Score,
                    Question = e.Question,
                    Answer = e.Answer
                }).ToList()
            };

            var aiResponse = await _aiService.UpdateRoadmapProgressAsync(aiRequest, cancellationToken, useCase: "UpdateRoadmapProgress");

            SurveyRoadmapDto updatedRoadmap;

            var aiPhaseCount = aiResponse?.Roadmap?.Phases?.Count ?? 0;
            var currentPhaseCount = currentRoadmap.Phases.Count;
            var aiStructureValid = aiResponse != null
                && string.Equals(aiResponse.Status, "success", StringComparison.OrdinalIgnoreCase)
                && aiPhaseCount > 0
                && aiPhaseCount == currentPhaseCount;

            if (!aiStructureValid && aiResponse != null && aiPhaseCount != currentPhaseCount)
            {
                _logger.LogWarning(
                    "UpdateRoadmapAfterInterview: AI response phase count mismatch for candidate {CandidateId} — expected {Expected}, got {Actual}; falling back to current roadmap",
                    candidateId, currentPhaseCount, aiPhaseCount);
            }

            if (aiStructureValid)
            {
                // Merge the mock history we built into the AI-returned roadmap
                // so the AI doesn't accidentally drop entries it didn't know about
                foreach (var phase in currentRoadmap.Phases)
                {
                    var aiPhase = aiResponse!.Roadmap!.Phases.FirstOrDefault(p => p.PhaseId == phase.PhaseId);
                    if (aiPhase != null)
                    {
                        aiPhase.MockHistory = phase.MockHistory;
                        aiPhase.RecommendedCoaches = phase.RecommendedCoaches;
                    }
                }

                updatedRoadmap = aiResponse!.Roadmap!;
            }
            else
            {
                _logger.LogWarning("UpdateRoadmapAfterInterview: AI progress update failed or returned empty for candidate {CandidateId}; keeping current roadmap with mock history only", candidateId);
                updatedRoadmap = currentRoadmap;
            }

            snapshot.Roadmap = MapRoadmap(updatedRoadmap);
            await _snapshotRepository.UpsertSnapshotAsync(snapshot, cancellationToken);

            _logger.LogInformation("UpdateRoadmapAfterInterview: roadmap updated for candidate {CandidateId} after room {RoomId}", candidateId, interviewRoomId);
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

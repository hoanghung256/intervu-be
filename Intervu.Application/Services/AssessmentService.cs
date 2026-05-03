using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.DTOs.Assessment;
using Intervu.Application.Interfaces.Services;
using Intervu.Application.Interfaces.UseCases.Notification;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
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
        private readonly ICoachProfileRepository _coachProfileRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAiService _aiService;
        private readonly IBackgroundService _jobService;
        private readonly IRoadmapPracticeEnrichmentService _roadmapPracticeEnrichment;
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

        private const int CoachCatalogLimit = 60;

        public AssessmentService(
            IUnitOfWork unitOfWork,
            IAiService aiService,
            IBackgroundService jobService,
            IRoadmapPracticeEnrichmentService roadmapPracticeEnrichment,
            ILogger<AssessmentService> logger)
        {
            _unitOfWork = unitOfWork;
            _snapshotRepository = unitOfWork.GetRepository<IUserSkillAssessmentSnapshotRepository>();
            _roomRepository = unitOfWork.GetRepository<IInterviewRoomRepository>();
            _coachProfileRepository = unitOfWork.GetRepository<ICoachProfileRepository>();
            _aiService = aiService;
            _jobService = jobService;
            _roadmapPracticeEnrichment = roadmapPracticeEnrichment;
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

        /// <summary>
        /// Maps the target career band ("junior", "middle", "senior", ...) to the
        /// numeric proficiency level (1-4) we expect a candidate at that band to hit.
        /// Used to classify a skill as "Weak" when 0 &lt; current &lt; band target.
        /// </summary>
        private static int MapTargetLevelBand(string? targetLevel)
        {
            return (targetLevel ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "intern" or "fresher" => 1,
                "junior" or "jr" => 2,
                "mid" or "middle" or "intermediate" => 3,
                "senior" or "sr" or "lead" or "principal" => 4,
                _ => 3
            };
        }

        // In-process cache for the matrix — survives the lifetime of the host. The
        // matrix is static configuration, so a service restart picks up changes;
        // we never invalidate during a request.
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, List<string>> _matrixSkillScopeCache = new();

        private async Task<List<string>?> TryFetchMatrixSkillScopeAsync(string role, string level, CancellationToken cancellationToken)
        {
            var key = $"{(role ?? string.Empty).Trim().ToLowerInvariant()}|{(level ?? string.Empty).Trim().ToLowerInvariant()}";
            if (_matrixSkillScopeCache.TryGetValue(key, out var cached))
            {
                return cached;
            }

            var matrix = await _aiService.GetCompetencyMatrixAsync(role ?? string.Empty, level ?? string.Empty, cancellationToken);
            if (matrix?.Skills == null || matrix.Skills.Count == 0)
            {
                return null;
            }

            var scope = matrix.Skills
                .Select(s => s.Skill)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            _matrixSkillScopeCache[key] = scope;
            return scope;
        }

        private async Task<List<string>> ResolveSkillScopeAsync(
            SurveyAnswerProfileDto profile,
            IReadOnlyCollection<SurveyAnswerResponseDto> responses,
            CancellationToken cancellationToken)
        {
            // Phase 2: prefer the AI-service competency matrix as single source of
            // truth. Fall back to the legacy keyword buckets if the matrix is
            // unreachable or has no entry for this (role, level), so a transient
            // AI-service outage doesn't break the assessment flow.
            var matrixScope = await TryFetchMatrixSkillScopeAsync(profile.Role ?? string.Empty, profile.Level ?? string.Empty, cancellationToken);
            var scoped = matrixScope ?? BuildLegacySkillScope(profile);

            // Always merge skills the candidate actually saw, so a question on
            // "Redis" outside the matrix still ends up in the snapshot.
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

        private static List<string> BuildLegacySkillScope(SurveyAnswerProfileDto profile)
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

            return baseSkills.Take(scopedCount).ToList();
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
            var skillScope = await ResolveSkillScopeAsync(answer.Profile, responses, cancellationToken);
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

            // A skill is "weak" when the candidate has SOME proficiency (level > 0)
            // but is still below the target band for their declared level. Without
            // this list, the roadmap LLM only sees "missing" gaps and treats partial
            // skills as already-satisfied — see Phase 1.3 of the audit plan.
            var targetBand = MapTargetLevelBand(answer.Profile.Level ?? target?.Level);
            var weak = currentSkills
                .Where(skill =>
                {
                    if (!int.TryParse(skill.Level, out var lvl)) return false;
                    return lvl > 0 && lvl < targetBand;
                })
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
                        Level = skill.Level,
                        Score = skill.Score
                    })
                    .ToList()
            };

            var snapshotGap = new Gap
            {
                Missing = missing,
                Weak = weak
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
                            Score = skill.Score,
                        })
                        .ToList(),
                },
                Gap = new AiGapDto
                {
                    Weak = gap.Weak ?? new List<string>(),
                    Missing = gap.Missing ?? new List<string>(),
                },
                CoachCatalog = await BuildCoachCatalogAsync(),
                AnswerJson = ParseAnswerJsonForAi(snapshot.AnswerJson),
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

            try
            {
                await _roadmapPracticeEnrichment.EnrichChildSkillQuestionsAsync(aiResponse.Roadmap, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Roadmap practice enrichment failed; saving roadmap without linked question bank items.");
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

            var currentRoadmap = MapRoadmapToSurveyDto(snapshot.Roadmap)!;

            // Prefer the phase that owns the linked roadmap node; fall back to first phase
            // with incomplete nodes. This keeps mock history anchored to the node the
            // candidate actually booked against (roadmap-driven flow).
            var targetNodeId = room.RoadmapNodeId;
            SurveyRoadmapPhaseDto? owningPhase = null;
            if (!string.IsNullOrWhiteSpace(targetNodeId))
            {
                owningPhase = currentRoadmap.Phases
                    .FirstOrDefault(p => p.Nodes.Any(n => n.SkillId == targetNodeId));
            }

            var activePhase = owningPhase
                ?? currentRoadmap.Phases
                    .FirstOrDefault(p => p.Nodes.Any(n => n.Assessment.Status != "Complete"))
                ?? currentRoadmap.Phases.Last();

            // Avoid duplicates: skip if this room was already recorded
            if (!activePhase.MockHistory.Any(m => m.MockId == mockEntry.MockId))
            {
                activePhase.MockHistory.Add(mockEntry);
            }

            var checkpointNode = ResolveCheckpointNode(activePhase, targetNodeId);

            // Checkpoint-linked rooms are authoritative gates for phase progression.
            // Their rubric result updates the checkpoint and phase without an LLM call.
            if (checkpointNode != null)
            {
                var evaluation = BuildRubricEvaluation(interviewRoomId, room.EvaluationResults, checkpointNode.Checkpoint?.PassThreshold ?? 70);
                ApplyCheckpointEvaluation(currentRoadmap, activePhase, checkpointNode, evaluation);

                snapshot.Roadmap = MapRoadmap(currentRoadmap);
                await _snapshotRepository.StageUpsertSnapshotAsync(snapshot, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                EnqueueRoadmapProgressNotification(candidateId, interviewRoomId, evaluation, activePhase);

                _logger.LogInformation(
                    "UpdateRoadmapAfterInterview: checkpoint {NodeId} evaluated as {Status} ({Score}%) for candidate {CandidateId} after room {RoomId}",
                    GetNodeKey(checkpointNode), evaluation.Status, evaluation.TotalPercentage, candidateId, interviewRoomId);
                return;
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
                }).ToList(),
                TargetNodeId = targetNodeId
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
            await _snapshotRepository.StageUpsertSnapshotAsync(snapshot, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            EnqueueRoadmapUpdatedNotification(candidateId, interviewRoomId);

            _logger.LogInformation("UpdateRoadmapAfterInterview: roadmap updated for candidate {CandidateId} after room {RoomId}", candidateId, interviewRoomId);
        }

        private static SurveyRoadmapNodeDto? ResolveCheckpointNode(SurveyRoadmapPhaseDto phase, string? targetNodeId)
        {
            if (!string.IsNullOrWhiteSpace(targetNodeId))
            {
                var linkedNode = phase.Nodes.FirstOrDefault(n =>
                    string.Equals(n.NodeId, targetNodeId, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(n.SkillId, targetNodeId, StringComparison.OrdinalIgnoreCase));

                if (linkedNode != null && IsLiveCheckpoint(linkedNode))
                {
                    return linkedNode;
                }
            }

            return phase.Nodes.FirstOrDefault(IsLiveCheckpoint);
        }

        private static bool IsLiveCheckpoint(SurveyRoadmapNodeDto node)
        {
            return string.Equals(node.PillarType, PillarType.LIVE_CHECKPOINT.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        private static string GetNodeKey(SurveyRoadmapNodeDto node)
        {
            return string.IsNullOrWhiteSpace(node.NodeId) ? node.SkillId : node.NodeId;
        }

        private static SurveyRubricEvaluationDto BuildRubricEvaluation(
            Guid interviewRoomId,
            IEnumerable<EvaluationResult> evaluations,
            int passThreshold)
        {
            var items = evaluations
                .Select(e => new SurveyRoadmapEvaluationDto
                {
                    Type = e.Type,
                    Score = e.Score,
                    Question = e.Question,
                    Answer = e.Answer,
                })
                .ToList();

            var scored = items.Where(item => item.Score > 0).ToList();
            var percentage = scored.Count == 0
                ? 0
                : (int)Math.Round(scored.Average(item => item.Score) / 10.0 * 100.0);
            percentage = Math.Clamp(percentage, 0, 100);
            passThreshold = passThreshold <= 0 ? 70 : passThreshold;

            return new SurveyRubricEvaluationDto
            {
                InterviewRoomId = interviewRoomId.ToString(),
                SubmittedAt = DateTime.UtcNow.ToString("o"),
                TotalPercentage = percentage,
                PassThreshold = passThreshold,
                Status = percentage >= passThreshold ? "Passed" : "Needs Improvement",
                LowScoreCategories = items
                    .Where(item => item.Score > 0 && item.Score < 7)
                    .OrderBy(item => item.Score)
                    .Select(item => string.IsNullOrWhiteSpace(item.Type) ? item.Question : item.Type)
                    .Where(label => !string.IsNullOrWhiteSpace(label))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(3)
                    .ToList(),
                Items = items,
            };
        }

        private static void ApplyCheckpointEvaluation(
            SurveyRoadmapDto roadmap,
            SurveyRoadmapPhaseDto phase,
            SurveyRoadmapNodeDto checkpointNode,
            SurveyRubricEvaluationDto evaluation)
        {
            if (checkpointNode.Checkpoint?.RubricEvaluation?.InterviewRoomId == evaluation.InterviewRoomId)
            {
                return;
            }

            checkpointNode.Checkpoint ??= new SurveyRoadmapCheckpointDto
            {
                Title = string.IsNullOrWhiteSpace(checkpointNode.SkillName)
                    ? "Live Mock Interview Checkpoint"
                    : checkpointNode.SkillName,
                Objective = "Validate readiness across this phase's hard and soft skills.",
                PassThreshold = evaluation.PassThreshold,
            };

            checkpointNode.Checkpoint.InterviewRoomId = evaluation.InterviewRoomId;
            checkpointNode.Checkpoint.PassThreshold = evaluation.PassThreshold;
            checkpointNode.Checkpoint.RubricEvaluation = evaluation;
            checkpointNode.Assessment.Progress = evaluation.TotalPercentage;
            checkpointNode.Assessment.Score = evaluation.TotalPercentage;
            checkpointNode.Assessment.Status = evaluation.Status;

            phase.CheckpointEvaluation = evaluation;
            phase.Status = evaluation.Status;

            if (evaluation.TotalPercentage >= evaluation.PassThreshold)
            {
                phase.IsUnlocked = true;
                phase.PassedAt = evaluation.SubmittedAt;
            }
        }

        private void EnqueueRoadmapUpdatedNotification(Guid candidateId, Guid interviewRoomId)
        {
            _jobService.Enqueue<INotificationUseCase>(uc => uc.CreateAsync(
                candidateId,
                NotificationType.RoadmapUpdated,
                "Roadmap updated",
                "Your roadmap has been refreshed based on your latest interview.",
                "/roadmap",
                interviewRoomId));
        }

        private void EnqueueRoadmapProgressNotification(
            Guid candidateId,
            Guid interviewRoomId,
            SurveyRubricEvaluationDto evaluation,
            SurveyRoadmapPhaseDto phase)
        {
            var passed = evaluation.TotalPercentage >= evaluation.PassThreshold;
            _jobService.Enqueue<INotificationUseCase>(uc => uc.CreateAsync(
                candidateId,
                NotificationType.RoadmapUpdated,
                passed ? "Phase passed" : "Checkpoint needs improvement",
                passed
                    ? $"You passed {phase.PhaseName} with {evaluation.TotalPercentage}%! Great work."
                    : $"Your checkpoint score was {evaluation.TotalPercentage}%. Review the suggested focus areas and try again.",
                "/roadmap",
                interviewRoomId));
        }

        /// <summary>
        /// Deterministic node progress formula — mirrors the AI service's
        /// node-targeted branch. progress = round(avg_score / 10 * 100), clamped to
        /// the current progress (monotonic). Status: >=80 Complete, >=40 Weak, else Missing.
        /// </summary>
        private static void ApplyDeterministicNodeUpdate(SurveyRoadmapNodeDto node, IEnumerable<EvaluationResult> evaluations)
        {
            var scored = evaluations?.Where(e => e.Score > 0).Select(e => e.Score).ToList() ?? new List<int>();
            if (scored.Count == 0) return;

            var avgScore = scored.Average();
            var newProgress = (int)Math.Round(avgScore / 10.0 * 100.0);
            if (newProgress < 0) newProgress = 0;
            if (newProgress > 100) newProgress = 100;

            node.Assessment.Progress = Math.Max(node.Assessment.Progress, newProgress);
            node.Assessment.Status = node.Assessment.Progress >= 80
                ? "Complete"
                : node.Assessment.Progress >= 40 ? "Weak" : "Missing";
        }

        /// <summary>
        /// The snapshot persists AnswerJson as a string blob. The AI service expects
        /// a real JSON object on the wire (so its strategy module can read
        /// `responses[].score`, `desc`, etc.), so we parse and forward as a JsonElement.
        /// Returns null when the blob is empty or unparseable — the AI service then
        /// falls back to building missions without per-question colour.
        /// </summary>
        private static object? ParseAnswerJsonForAi(string? answerJson)
        {
            if (string.IsNullOrWhiteSpace(answerJson) || answerJson.Trim() == "{}")
            {
                return null;
            }

            try
            {
                using var document = JsonDocument.Parse(answerJson);
                return document.RootElement.Clone();
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private async Task<List<AiCoachCatalogEntryDto>> BuildCoachCatalogAsync()
        {
            try
            {
                var coaches = await _coachProfileRepository.GetCoachCatalogForRoadmapAsync(CoachCatalogLimit);
                return coaches
                    .Select(c => new AiCoachCatalogEntryDto
                    {
                        Id = c.Id.ToString(),
                        Name = c.User?.FullName ?? string.Empty,
                        SlugProfileUrl = c.User?.SlugProfileUrl ?? string.Empty,
                        AvatarUrl = c.User?.ProfilePicture ?? string.Empty,
                        Skills = (c.Skills ?? new List<Skill>()).Select(s => s.Name).Where(n => !string.IsNullOrWhiteSpace(n)).ToList(),
                        Bio = c.Bio ?? string.Empty,
                        Services = (c.InterviewServices ?? new List<CoachInterviewService>())
                            .Where(s => s.InterviewType?.Status == InterviewTypeStatus.Active)
                            .Select(s => new AiCoachCatalogServiceDto
                            {
                                Id = s.Id.ToString(),
                                InterviewTypeName = s.InterviewType?.Name ?? string.Empty,
                                Price = s.Price,
                                DurationMinutes = s.DurationMinutes,
                                AimLevelHint = string.Empty,
                            })
                            .ToList(),
                    })
                    .Where(entry => entry.Services.Count > 0)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "BuildCoachCatalogAsync: failed to assemble coach catalog; proceeding without per-node recommendations");
                return new List<AiCoachCatalogEntryDto>();
            }
        }

        private static RoadmapSnapshot? MapRoadmap(SurveyRoadmapDto? roadmap)
        {
            if (roadmap == null)
            {
                return null;
            }

            return new RoadmapSnapshot
            {
                SchemaVersion = roadmap.SchemaVersion <= 0 ? 2 : roadmap.SchemaVersion,
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
                        PhaseDescription = phase.PhaseDescription,
                        Status = NormalizeRoadmapPhaseStatusForApi(phase.Status),
                        IsUnlocked = true,
                        UnlockedAt = phase.UnlockedAt,
                        PassedAt = phase.PassedAt,
                        CheckpointEvaluation = MapRubricEvaluation(phase.CheckpointEvaluation),
                        RecommendedCoach = phase.RecommendedCoach == null ? null : new RoadmapNodeCoachSnapshot
                        {
                            Id = phase.RecommendedCoach.Id,
                            Name = phase.RecommendedCoach.Name,
                            SlugProfileUrl = phase.RecommendedCoach.SlugProfileUrl,
                            AvatarUrl = phase.RecommendedCoach.AvatarUrl,
                        },
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
                            .Select(node =>
                            {
                                var pillar = ParsePillarType(node.PillarType);
                                return new RoadmapNodeSnapshot
                                {
                                    NodeId = string.IsNullOrWhiteSpace(node.NodeId) ? node.SkillId : node.NodeId,
                                    SkillId = node.SkillId,
                                    SkillName = node.SkillName,
                                    PillarType = pillar,
                                    Checkpoint = node.Checkpoint == null ? null : new RoadmapCheckpointSnapshot
                                    {
                                        Title = node.Checkpoint.Title,
                                        Objective = node.Checkpoint.Objective,
                                        PassThreshold = node.Checkpoint.PassThreshold,
                                        InterviewRoomId = node.Checkpoint.InterviewRoomId,
                                        RubricEvaluation = MapRubricEvaluation(node.Checkpoint.RubricEvaluation),
                                    },
                                    Assessment = new RoadmapNodeAssessmentSnapshot
                                    {
                                        CurrentLevel = node.Assessment?.CurrentLevel ?? string.Empty,
                                        TargetLevel = node.Assessment?.TargetLevel ?? string.Empty,
                                        SfiaLevel = node.Assessment?.SfiaLevel ?? 0,
                                        Status = NormalizeRoadmapNodeAssessmentStatusForApi(
                                            pillar,
                                            node.Assessment?.Status),
                                        Progress = node.Assessment?.Progress ?? 0,
                                        Score = node.Assessment?.Score ?? 0,
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
                                RecommendedCoach = node.RecommendedCoach == null ? null : new RoadmapNodeCoachSnapshot
                                {
                                    Id = node.RecommendedCoach.Id,
                                    Name = node.RecommendedCoach.Name,
                                    SlugProfileUrl = node.RecommendedCoach.SlugProfileUrl,
                                    AvatarUrl = node.RecommendedCoach.AvatarUrl,
                                },
                                };
                            })
                            .ToList(),
                    })
                    .ToList(),
            };
        }

        /// <summary>
        /// Legacy snapshots used Locked phases; product policy allows learning in any order.
        /// Normalize on read so API clients never receive gated phase status.
        /// </summary>
        private static string NormalizeRoadmapPhaseStatusForApi(string? status)
        {
            if (string.IsNullOrWhiteSpace(status) ||
                string.Equals(status, "Locked", StringComparison.OrdinalIgnoreCase))
            {
                return "Unlocked";
            }

            return status;
        }

        /// <summary>
        /// Legacy checkpoints used assessment status Locked; normalize so scheduling is never blocked.
        /// Other pillars: Locked historically meant "not yet available" — map to Missing.
        /// </summary>
        private static string NormalizeRoadmapNodeAssessmentStatusForApi(PillarType pillarType, string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return string.Empty;
            }

            if (!string.Equals(status, "Locked", StringComparison.OrdinalIgnoreCase))
            {
                return status;
            }

            return pillarType == PillarType.LIVE_CHECKPOINT ? "Unlocked" : "Missing";
        }

        private static SurveyRoadmapDto? MapRoadmapToSurveyDto(RoadmapSnapshot? roadmap)
        {
            if (roadmap == null)
            {
                return null;
            }

            return new SurveyRoadmapDto
            {
                SchemaVersion = roadmap.SchemaVersion <= 0 ? 1 : roadmap.SchemaVersion,
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
                        PhaseDescription = phase.PhaseDescription,
                        Status = NormalizeRoadmapPhaseStatusForApi(phase.Status),
                        IsUnlocked = true,
                        UnlockedAt = phase.UnlockedAt,
                        PassedAt = phase.PassedAt,
                        CheckpointEvaluation = MapRubricEvaluation(phase.CheckpointEvaluation),
                        RecommendedCoach = phase.RecommendedCoach == null ? null : new SurveyRoadmapNodeCoachDto
                        {
                            Id = phase.RecommendedCoach.Id,
                            Name = phase.RecommendedCoach.Name,
                            SlugProfileUrl = phase.RecommendedCoach.SlugProfileUrl,
                            AvatarUrl = phase.RecommendedCoach.AvatarUrl,
                        },
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
                                NodeId = string.IsNullOrWhiteSpace(node.NodeId) ? node.SkillId : node.NodeId,
                                SkillId = node.SkillId,
                                SkillName = node.SkillName,
                                PillarType = node.PillarType.ToString(),
                                Checkpoint = node.Checkpoint == null ? null : new SurveyRoadmapCheckpointDto
                                {
                                    Title = node.Checkpoint.Title,
                                    Objective = node.Checkpoint.Objective,
                                    PassThreshold = node.Checkpoint.PassThreshold,
                                    InterviewRoomId = node.Checkpoint.InterviewRoomId,
                                    RubricEvaluation = MapRubricEvaluation(node.Checkpoint.RubricEvaluation),
                                },
                                Assessment = new SurveyRoadmapNodeAssessmentDto
                                {
                                    CurrentLevel = node.Assessment?.CurrentLevel ?? string.Empty,
                                    TargetLevel = node.Assessment?.TargetLevel ?? string.Empty,
                                    SfiaLevel = node.Assessment?.SfiaLevel ?? 0,
                                    Status = NormalizeRoadmapNodeAssessmentStatusForApi(
                                        node.PillarType,
                                        node.Assessment?.Status),
                                    Progress = node.Assessment?.Progress ?? 0,
                                    Score = node.Assessment?.Score ?? 0,
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
                                RecommendedCoach = node.RecommendedCoach == null ? null : new SurveyRoadmapNodeCoachDto
                                {
                                    Id = node.RecommendedCoach.Id,
                                    Name = node.RecommendedCoach.Name,
                                    SlugProfileUrl = node.RecommendedCoach.SlugProfileUrl,
                                    AvatarUrl = node.RecommendedCoach.AvatarUrl,
                                },
                            })
                            .ToList(),
                    })
                    .ToList(),
            };
        }

        private static PillarType ParsePillarType(string? value)
        {
            return Enum.TryParse<PillarType>((value ?? string.Empty).Trim(), ignoreCase: true, out var parsed)
                ? parsed
                : PillarType.HARD_SKILL;
        }

        private static RoadmapCheckpointSnapshot? MapCheckpoint(SurveyRoadmapCheckpointDto? checkpoint)
        {
            if (checkpoint == null)
            {
                return null;
            }

            return new RoadmapCheckpointSnapshot
            {
                Title = checkpoint.Title,
                Objective = checkpoint.Objective,
                PassThreshold = checkpoint.PassThreshold <= 0 ? 70 : checkpoint.PassThreshold,
                InterviewRoomId = checkpoint.InterviewRoomId,
                RubricEvaluation = MapRubricEvaluation(checkpoint.RubricEvaluation),
            };
        }

        private static SurveyRoadmapCheckpointDto? MapCheckpoint(RoadmapCheckpointSnapshot? checkpoint)
        {
            if (checkpoint == null)
            {
                return null;
            }

            return new SurveyRoadmapCheckpointDto
            {
                Title = checkpoint.Title,
                Objective = checkpoint.Objective,
                PassThreshold = checkpoint.PassThreshold <= 0 ? 70 : checkpoint.PassThreshold,
                InterviewRoomId = checkpoint.InterviewRoomId,
                RubricEvaluation = MapRubricEvaluation(checkpoint.RubricEvaluation),
            };
        }

        private static RubricEvaluationSnapshot? MapRubricEvaluation(SurveyRubricEvaluationDto? evaluation)
        {
            if (evaluation == null)
            {
                return null;
            }

            return new RubricEvaluationSnapshot
            {
                InterviewRoomId = evaluation.InterviewRoomId,
                SubmittedAt = evaluation.SubmittedAt,
                TotalPercentage = evaluation.TotalPercentage,
                PassThreshold = evaluation.PassThreshold <= 0 ? 70 : evaluation.PassThreshold,
                Status = evaluation.Status,
                LowScoreCategories = evaluation.LowScoreCategories ?? new List<string>(),
                Items = (evaluation.Items ?? new List<SurveyRoadmapEvaluationDto>())
                    .Select(item => new RubricEvaluationItemSnapshot
                    {
                        Type = item.Type,
                        Score = item.Score,
                        Question = item.Question,
                        Answer = item.Answer,
                    })
                    .ToList(),
            };
        }

        private static SurveyRubricEvaluationDto? MapRubricEvaluation(RubricEvaluationSnapshot? evaluation)
        {
            if (evaluation == null)
            {
                return null;
            }

            return new SurveyRubricEvaluationDto
            {
                InterviewRoomId = evaluation.InterviewRoomId,
                SubmittedAt = evaluation.SubmittedAt,
                TotalPercentage = evaluation.TotalPercentage,
                PassThreshold = evaluation.PassThreshold <= 0 ? 70 : evaluation.PassThreshold,
                Status = evaluation.Status,
                LowScoreCategories = evaluation.LowScoreCategories ?? new List<string>(),
                Items = (evaluation.Items ?? new List<RubricEvaluationItemSnapshot>())
                    .Select(item => new SurveyRoadmapEvaluationDto
                    {
                        Type = item.Type,
                        Score = item.Score,
                        Question = item.Question,
                        Answer = item.Answer,
                    })
                    .ToList(),
            };
        }

    }
}

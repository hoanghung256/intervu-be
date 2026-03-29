using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
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

        public AssessmentService(IUserSkillAssessmentSnapshotRepository snapshotRepository)
        {
            _snapshotRepository = snapshotRepository;
        }

        // Raw answers are not stored; only processed survey summaries are saved.

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
                Gap = JsonSerializer.Serialize(userSkillAssessment.Gap)
            };
        }
    }
}

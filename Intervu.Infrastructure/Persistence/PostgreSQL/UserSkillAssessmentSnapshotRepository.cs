using System;
using System.Threading.Tasks;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Repositories
{
    public class UserSkillAssessmentSnapshotRepository : IUserSkillAssessmentSnapshotRepository
    {
        private readonly IntervuPostgreDbContext _context;

        public UserSkillAssessmentSnapshotRepository(IntervuPostgreDbContext context)
        {
            _context = context;
        }

        public async Task<UserSkillAssessmentSnapshot?> GetUserSkillAssessmentById(Guid userId)
        {
            return await _context.UserSkillAssessments
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task UpsertSnapshotAsync(UserSkillAssessmentSnapshot snapshot)
        {
            var now = DateTime.UtcNow;
            snapshot.EnsureJsonPayloads();
            var hasIncomingAnswerJson = HasMeaningfulJsonPayload(snapshot.AnswerJson);

            var existing = await _context.UserSkillAssessments
                .FirstOrDefaultAsync(x => x.UserId == snapshot.UserId);

            if (existing == null)
            {
                snapshot.Id = snapshot.Id == Guid.Empty ? Guid.NewGuid() : snapshot.Id;
                snapshot.AnswerJson = hasIncomingAnswerJson ? snapshot.AnswerJson : null;
                snapshot.CreatedAt = now;
                snapshot.UpdatedAt = now;
                await _context.UserSkillAssessments.AddAsync(snapshot);
            }
            else
            {
                existing.UserId = snapshot.UserId;
                existing.TargetJson = snapshot.TargetJson;
                existing.CurrentJson = snapshot.CurrentJson;
                existing.GapJson = snapshot.GapJson;
                existing.RoadMapJson = snapshot.RoadMapJson;
                if (hasIncomingAnswerJson)
                {
                    existing.AnswerJson = snapshot.AnswerJson;
                }
                existing.UpdatedAt = now;
            }

            await _context.SaveChangesAsync();
        }

        public async Task SaveAnswerJsonAsync(Guid userId, string answerJson)
        {
            var now = DateTime.UtcNow;
            var existing = await _context.UserSkillAssessments
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (existing == null)
            {
                await _context.UserSkillAssessments.AddAsync(new UserSkillAssessmentSnapshot
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    AnswerJson = answerJson,
                    TargetJson = "{}",
                    CurrentJson = "{}",
                    GapJson = "{}",
                    RoadMapJson = "{}",
                    CreatedAt = now,
                    UpdatedAt = now,
                });
            }
            else
            {
                existing.AnswerJson = answerJson;
                existing.UpdatedAt = now;
            }

            await _context.SaveChangesAsync();
        }

        private static bool HasMeaningfulJsonPayload(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            var trimmed = json.Trim();
            return trimmed != "{}" && !string.Equals(trimmed, "null", StringComparison.OrdinalIgnoreCase);
        }
    }
}

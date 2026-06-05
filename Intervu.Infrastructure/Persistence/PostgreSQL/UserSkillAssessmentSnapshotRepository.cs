using System;
using System.Threading;
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

        public async Task<UserSkillAssessmentSnapshot?> GetUserSkillAssessmentById(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.UserSkillAssessments
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        }

        public async Task UpsertSnapshotAsync(UserSkillAssessmentSnapshot snapshot, CancellationToken cancellationToken = default)
        {
            await StageUpsertSnapshotAsync(snapshot, cancellationToken);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Keep legacy callers resilient to concurrent snapshot creation.
                _context.ChangeTracker.Clear();
                await StageUpsertSnapshotAsync(snapshot, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task StageUpsertSnapshotAsync(UserSkillAssessmentSnapshot snapshot, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            snapshot.EnsureJsonPayloads();
            snapshot.AnswerJson = NormalizeJsonPayload(snapshot.AnswerJson);

            var existing = await _context.UserSkillAssessments
                .FirstOrDefaultAsync(x => x.UserId == snapshot.UserId, cancellationToken);

            if (existing == null)
            {
                snapshot.Id = snapshot.Id == Guid.Empty ? Guid.NewGuid() : snapshot.Id;
                snapshot.CreatedAt = now;
                snapshot.UpdatedAt = now;
                await _context.UserSkillAssessments.AddAsync(snapshot, cancellationToken);
            }
            else
            {
                existing.UserId = snapshot.UserId;
                existing.TargetJson = snapshot.TargetJson;
                existing.CurrentJson = snapshot.CurrentJson;
                existing.GapJson = snapshot.GapJson;
                existing.RoadMapJson = snapshot.RoadMapJson;
                existing.AnswerJson = snapshot.AnswerJson;
                existing.UpdatedAt = now;
            }
        }

        public async Task SaveAnswerJsonAsync(Guid userId, string answerJson, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var normalizedAnswerJson = NormalizeJsonPayload(answerJson);
            var existing = await _context.UserSkillAssessments
                .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

            UserSkillAssessmentSnapshot? added = null;

            if (existing == null)
            {
                added = new UserSkillAssessmentSnapshot
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    AnswerJson = normalizedAnswerJson,
                    TargetJson = "{}",
                    CurrentJson = "{}",
                    GapJson = "{}",
                    RoadMapJson = "{}",
                    CreatedAt = now,
                    UpdatedAt = now,
                };
                await _context.UserSkillAssessments.AddAsync(added, cancellationToken);
            }
            else
            {
                existing.AnswerJson = normalizedAnswerJson;
                existing.UpdatedAt = now;
            }

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException) when (existing == null)
            {
                // Concurrent insert conflict — detach and retry as update.
                if (added != null)
                {
                    _context.Entry(added).State = EntityState.Detached;
                }
                var conflicting = await _context.UserSkillAssessments
                    .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
                if (conflicting != null)
                {
                    conflicting.AnswerJson = normalizedAnswerJson;
                    conflicting.UpdatedAt = now;
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }
        }

        public async Task<bool> DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var existing = await _context.UserSkillAssessments
                .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

            if (existing == null)
            {
                return false;
            }

            _context.UserSkillAssessments.Remove(existing);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
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

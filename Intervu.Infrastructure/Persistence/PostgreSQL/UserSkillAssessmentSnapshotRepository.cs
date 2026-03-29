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

            var existing = await _context.UserSkillAssessments
                .FirstOrDefaultAsync(x => x.UserId == snapshot.UserId);

            if (existing == null)
            {
                snapshot.Id = snapshot.Id == Guid.Empty ? Guid.NewGuid() : snapshot.Id;
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
                existing.UpdatedAt = now;
            }

            await _context.SaveChangesAsync();
        }
    }
}

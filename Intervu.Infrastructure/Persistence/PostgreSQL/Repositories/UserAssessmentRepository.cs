using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Intervu.Application.Interfaces.Repositories;
using Intervu.Domain.Entities;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Repositories
{
    public class UserAssessmentRepository : IUserAssessmentRepository
    {
        private readonly IntervuPostgreDbContext _db;

        public UserAssessmentRepository(IntervuPostgreDbContext db)
        {
            _db = db;
        }

        public async Task AddAnswersAsync(IEnumerable<UserAssessmentAnswer> answers)
        {
            await _db.UserAssessmentAnswers.AddRangeAsync(answers);
            await _db.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<UserAssessmentAnswer>> GetAnswersByAssessmentIdAsync(Guid assessmentId)
        {
            return await _db.UserAssessmentAnswers
                .AsNoTracking()
                .Where(x => x.AssessmentId == assessmentId)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Skill>> GetAllSkillsAsync()
        {
            return await _db.Skills
                .AsNoTracking()
                .ToListAsync();
        }

        
    }
}

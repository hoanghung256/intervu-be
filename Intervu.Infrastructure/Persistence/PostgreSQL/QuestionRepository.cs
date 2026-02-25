using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class QuestionRepository : RepositoryBase<Question>, IQuestionRepository
    {
        public QuestionRepository(IntervuPostgreDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Question>> GetByExperienceIdAsync(Guid interviewExperienceId)
        {
            return await _context.Questions
                .Where(q => q.InterviewExperienceId == interviewExperienceId)
                .OrderBy(q => q.CreatedAt)
                .ToListAsync();
        }
    }
}

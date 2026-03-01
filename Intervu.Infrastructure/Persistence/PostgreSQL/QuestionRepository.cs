using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
                .Include(q => q.Comments.OrderByDescending(c => c.IsAnswer).ThenByDescending(c => c.Vote).ThenBy(c => c.CreatedAt))
                .Where(q => q.InterviewExperienceId == interviewExperienceId)
                .OrderBy(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<(List<Question> Items, int TotalCount)> GetPagedAsync(
            string? searchTerm,
            string? questionType,
            string? role,
            ExperienceLevel? level,
            int page,
            int pageSize)
        {
            var query = _context.Questions
                .Include(q => q.InterviewExperience)
                .Include(q => q.Comments.OrderByDescending(c => c.IsAnswer).ThenByDescending(c => c.Vote).ThenBy(c => c.CreatedAt))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(q => q.Content.Contains(searchTerm));

            if (!string.IsNullOrWhiteSpace(questionType))
                query = query.Where(q => q.QuestionType == questionType);

            if (!string.IsNullOrWhiteSpace(role))
                query = query.Where(q => q.InterviewExperience.Role == role);

            if (level.HasValue)
                query = query.Where(q => q.InterviewExperience.Level == level);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(q => q.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<Question?> GetDetailAsync(Guid id)
        {
            return await _context.Questions
                .Include(q => q.InterviewExperience)
                    .ThenInclude(e => e.User)
                .Include(q => q.Comments.OrderByDescending(c => c.IsAnswer).ThenByDescending(c => c.Vote).ThenBy(c => c.CreatedAt))
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<List<Question>> GetRelatedAsync(Guid excludeId, string questionType, string role, int limit)
        {
            return await _context.Questions
                .Include(q => q.InterviewExperience)
                .Where(q => q.Id != excludeId &&
                            (q.QuestionType == questionType || q.InterviewExperience.Role == role))
                .OrderByDescending(q => q.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }
    }
}


using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Entities.Constants.QuestionConstants;
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
            Guid? companyId,
            Guid? tagId,
            QuestionCategory? category,
            Role? role,
            ExperienceLevel? level,
            Intervu.Domain.Entities.Constants.QuestionConstants.InterviewRound? round,
            SortOption? sortBy,
            int page,
            int pageSize,
            QuestionStatus? status = null)
        {
            var query = _context.Questions
                .Include(q => q.QuestionCompanies).ThenInclude(qc => qc.Company)
                .Include(q => q.QuestionRoles)
                .Include(q => q.QuestionTags).ThenInclude(qt => qt.Tag)
                .Include(q => q.Comments)
                .AsQueryable();

            // Moderation visibility:
            // - If a status is explicitly requested (admin moderation view), filter by that status only.
            // - Otherwise enforce the public contract: only Approved + not hidden are visible.
            if (status.HasValue)
            {
                var s = status.Value;
                query = query.Where(q => q.Status == s);
            }
            else
            {
                query = query.Where(q => q.Status == QuestionStatus.Approved && q.IsHidden == false);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(q =>
                    q.Title.Contains(searchTerm) ||
                    q.Content.Contains(searchTerm) ||
                    q.QuestionCompanies.Any(qc => qc.Company.Name.Contains(searchTerm)));

            if (companyId.HasValue)
                query = query.Where(q => q.QuestionCompanies.Any(qc => qc.CompanyId == companyId.Value));

            if (tagId.HasValue)
                query = query.Where(q => q.QuestionTags.Any(qt => qt.TagId == tagId.Value));

            if (category.HasValue)
                query = query.Where(q => q.Category == category.Value);

            if (role.HasValue)
                query = query.Where(q => q.QuestionRoles.Any(qr => qr.Role == role.Value));

            if (level.HasValue)
                query = query.Where(q => q.Level == level.Value);

            if (round.HasValue)
                query = query.Where(q => q.Round == round.Value);

            var total = await query.CountAsync();

            IQueryable<Question> sorted = sortBy switch
            {
                SortOption.Hot => query.OrderByDescending(q => q.IsHot).ThenByDescending(q => q.ViewCount),
                SortOption.Top => query.OrderByDescending(q => q.Comments.Count)
                                       .ThenByDescending(q => q.ViewCount),
                _ => query.OrderByDescending(q => q.CreatedAt)
            };

            var items = await sorted
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<List<Question>> SearchAsync(string keyword, int limit = 10)
        {
            return await _context.Questions
                .Include(q => q.QuestionCompanies).ThenInclude(qc => qc.Company)
                .Include(q => q.QuestionRoles)
                .Include(q => q.QuestionTags).ThenInclude(qt => qt.Tag)
                .Where(q => q.Status == QuestionStatus.Approved && q.IsHidden == false)
                .Where(q => q.Title.ToLower().Contains(keyword.ToLower()) ||
                             q.Content.ToLower().Contains(keyword.ToLower()))
                .OrderByDescending(q => q.Comments.Count)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<Question?> GetDetailAsync(Guid id)
        {
            return await _context.Questions
                .Include(q => q.Author)
                .Include(q => q.QuestionCompanies).ThenInclude(qc => qc.Company)
                .Include(q => q.QuestionRoles)
                .Include(q => q.QuestionTags).ThenInclude(qt => qt.Tag)
                .Include(q => q.Comments)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<List<Question>> GetRelatedAsync(Guid excludeId, Guid questionId, int limit)
        {
            // Find tags and roles of the source question
            var tagIds = await _context.QuestionTags
                .Where(qt => qt.QuestionId == questionId)
                .Select(qt => qt.TagId)
                .ToListAsync();

            var roles = await _context.QuestionRoles
                .Where(qr => qr.QuestionId == questionId)
                .Select(qr => qr.Role)
                .ToListAsync();

            return await _context.Questions
                .Include(q => q.QuestionCompanies).ThenInclude(qc => qc.Company)
                .Include(q => q.QuestionRoles)
                .Where(q => q.Id != excludeId &&
                    (q.QuestionTags.Any(qt => tagIds.Contains(qt.TagId)) ||
                     q.QuestionRoles.Any(qr => roles.Contains(qr.Role))))
                .OrderByDescending(q => q.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task IncrementViewCountAsync(Guid questionId)
        {
            await _context.Questions
                .Where(q => q.Id == questionId)
                .ExecuteUpdateAsync(s => s.SetProperty(q => q.ViewCount, q => q.ViewCount + 1));
        }

        public async Task<List<Question>> GetByIdsAsync(IEnumerable<Guid> ids)
        {
            var idList = ids.ToList();
            return await _context.Questions
                .Where(q => idList.Contains(q.Id))
                .Include(q => q.Author)
                .Include(q => q.QuestionCompanies).ThenInclude(qc => qc.Company)
                .Include(q => q.QuestionRoles)
                .Include(q => q.QuestionTags).ThenInclude(qt => qt.Tag)
                .ToListAsync();
        }
    }
}


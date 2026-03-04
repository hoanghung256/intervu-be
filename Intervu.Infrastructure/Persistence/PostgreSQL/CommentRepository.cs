using Intervu.Domain.Entities;
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
    public class CommentRepository : RepositoryBase<Comment>, ICommentRepository
    {
        public CommentRepository(IntervuPostgreDbContext context) : base(context)
        {
        }

        public async Task<List<Comment>> GetByQuestionIdAsync(Guid questionId)
        {
            return await _context.Comments
                .Where(c => c.QuestionId == questionId)
                .OrderByDescending(c => c.IsAnswer)
                .ThenByDescending(c => c.Vote)
                .ThenBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<(List<Comment> Items, int TotalCount)> GetPagedByQuestionIdAsync(
            Guid questionId, int page, int pageSize, SortOption? sortBy = null)
        {
            var baseQuery = _context.Comments
                .Where(c => c.QuestionId == questionId);

            var total = await baseQuery.CountAsync();

            IQueryable<Comment> sorted = sortBy switch
            {
                SortOption.New => baseQuery.OrderByDescending(c => c.CreatedAt),
                SortOption.Top => baseQuery.OrderByDescending(c => c.Vote)
                                           .ThenByDescending(c => c.IsAnswer),
                _ => baseQuery.OrderByDescending(c => c.IsAnswer) // Hot (default)
                              .ThenByDescending(c => c.Vote)
                              .ThenBy(c => c.CreatedAt)
            };

            var items = await sorted
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }
    }
}

using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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

        public async Task<(List<Comment> Items, int TotalCount)> GetPagedByQuestionIdAsync(Guid questionId, int page, int pageSize)
        {
            var query = _context.Comments
                .Where(c => c.QuestionId == questionId)
                .OrderByDescending(c => c.IsAnswer)
                .ThenByDescending(c => c.Vote)
                .ThenBy(c => c.CreatedAt);

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }
    }
}

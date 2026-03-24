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
    public class QuestionReportRepository(IntervuPostgreDbContext context)
        : RepositoryBase<QuestionReport>(context), IQuestionReportRepository
    {
        public async Task<bool> HasPendingReportAsync(Guid questionId, Guid userId)
        {
            return await _context.QuestionReports
                .AnyAsync(r => r.QuestionId == questionId
                            && r.ReportedBy == userId
                            && r.Status == QuestionReportStatus.Pending);
        }

        public async Task<(List<QuestionReport> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, QuestionReportStatus? status = null, string? search = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _context.QuestionReports
                .Include(r => r.Question)
                .Include(r => r.Reporter)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(r => r.Reason.Contains(search) || r.Question.Title.Contains(search));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }
    }
}
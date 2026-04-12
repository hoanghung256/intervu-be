using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class InterviewReportRepository(IntervuPostgreDbContext context)
        : RepositoryBase<InterviewReport>(context), IInterviewReportRepository
    {
        public async Task<(List<InterviewReport> Items, int TotalCount)> GetPagedAsync(
            int page,
            int pageSize,
            InterviewReportStatus? status = null,
            string? search = null,
            Guid? reporterId = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _context.InterviewReports
                .Include(x => x.InterviewRoom)
                .Include(x => x.Reporter)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status.Value);
            }
            
            if (reporterId.HasValue)
            {
                query = query.Where(x => x.ReportedBy == reporterId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.Reason.Contains(search) ||
                    (x.Details != null && x.Details.Contains(search)) ||
                    (x.ExpectTo != null && x.ExpectTo.Contains(search)) ||
                    (x.Reporter != null && x.Reporter.FullName.Contains(search)));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<bool> ExistsByRoomIdAsync(Guid interviewRoomId)
        {
            return await _context.InterviewReports.AnyAsync(x => x.InterviewRoomId == interviewRoomId);
        }

        public async Task<InterviewReport?> GetByRoomIdAsync(Guid interviewRoomId)
        {
            return await _context.InterviewReports
                .Include(x => x.Reporter)
                .Where(x => x.InterviewRoomId == interviewRoomId)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetPendingCountAsync()
        {
            return await _context.InterviewReports.CountAsync(x => x.Status == InterviewReportStatus.Pending);
        }
    }
}

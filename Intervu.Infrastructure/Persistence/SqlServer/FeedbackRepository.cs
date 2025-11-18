using Intervu.Application.Common;
using Intervu.Application.Interfaces.Repositories;
using Intervu.Domain.Entities;
using Intervu.Infrastructure.Persistence.SqlServer.DataContext;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Intervu.Infrastructure.Persistence.SqlServer
{
    public class FeedbackRepository : RepositoryBase<Feedback>, IFeedbackRepository
    {
        public FeedbackRepository(IntervuDbContext context) : base(context)
        {
        }

        public async Task<PagedResult<Feedback>> GetPagedFeedbacksAsync(int page, int pageSize)
        {
            var query = _context.Feedbacks.AsQueryable();

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(f => f.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Feedback>(items, totalItems, pageSize, page);
        }

        public async Task<int> GetTotalFeedbacksCountAsync()
        {
            return await _context.Feedbacks.CountAsync();
        }

        public async Task<double> GetAverageRatingAsync()
        {
            if (!await _context.Feedbacks.AnyAsync())
                return 0;

            return await _context.Feedbacks.AverageAsync(f => f.Rating);
        }
    }
}

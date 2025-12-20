using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;


namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class FeedbackRepository(IntervuPostgreDbContext context) : RepositoryBase<Feedback>(context), IFeedbackRepository
    {
        public async Task CreateFeedbackAsync(Feedback feedback)
        {
            await AddAsync(feedback);
            await SaveChangesAsync();
        }

        public async Task<Feedback?> GetFeedbackByIdAsync(int id)
        {
            return await GetByIdAsync(id);
        }


        public async Task<(IReadOnlyList<Feedback> Items, int TotalCount)> GetFeedbacksByStudentIdAsync(int studentId, int page, int pageSize)
        {
            var query = _context.Feedbacks.Where(f => f.StudentId == studentId).AsQueryable();

            var totalItems = await query.CountAsync();

            var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
                .ToListAsync();

            return (items, totalItems);
        }

        public async Task UpdateFeedbackAsync(Feedback updatedFeedback)
        {
            UpdateAsync(updatedFeedback);
            await SaveChangesAsync();
        }

        public async Task<List<Feedback>> GetFeedbacksByInterviewRoomIdAsync(int interviewRoomId)
        {
            return await _context.Feedbacks
                .Where(f => f.InterviewRoomId == interviewRoomId)
                .ToListAsync();
        }

        public async Task<(IReadOnlyList<Feedback> Items, int TotalCount)> GetPagedFeedbacksAsync(int page, int pageSize)
        {
            var query = _context.Feedbacks.AsQueryable();

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalItems);
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

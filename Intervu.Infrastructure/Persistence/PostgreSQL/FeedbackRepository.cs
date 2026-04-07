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

        public async Task<Feedback?> GetFeedbackByIdAsync(Guid id)
        {
            return await GetByIdAsync(id);
        }


        public async Task<(IReadOnlyList<Feedback> Items, int TotalCount)> GetFeedbacksByCandidateIdAsync(Guid candidateId, int page, int pageSize)
        {
            var query = _context.Feedbacks.Include(f => f.CoachProfile).ThenInclude(cp => cp.User).Include(f => f.InterviewRoom).Where(f => f.CandidateId == candidateId).AsQueryable();

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

        public async Task<List<Feedback>> GetFeedbacksByInterviewRoomIdAsync(Guid interviewRoomId)
        {
            return await _context.Feedbacks
                .Where(f => f.InterviewRoomId == interviewRoomId)
                .ToListAsync();
        }

        public async Task<Dictionary<Guid, double?>> GetRatingsByInterviewRoomIdsAsync(IEnumerable<Guid> interviewRoomIds)
        {
            var ids = interviewRoomIds.Distinct().ToList();
            if (ids.Count == 0)
            {
                return new Dictionary<Guid, double?>();
            }

            var ratings = await _context.Feedbacks
                .Where(f => ids.Contains(f.InterviewRoomId))
                .Select(f => new { f.InterviewRoomId, f.Rating })
                .ToListAsync();

            return ratings
                .GroupBy(x => x.InterviewRoomId)
                .ToDictionary(g => g.Key, g => (double?)g.First().Rating);
        }

        public async Task<(IReadOnlyList<Feedback> Items, int TotalCount)> GetPagedFeedbacksAsync(int page, int pageSize)
        {
            var query = _context.Feedbacks.Include(f => f.CoachProfile).ThenInclude(cp => cp.User).Include(f => f.InterviewRoom).AsQueryable();

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

        public async Task<double> GetAverageRatingByCoachIdAsync(Guid coachId)
        {
            var average = await _context.Feedbacks.AverageAsync(f => f.Rating);
            return average;
        }

        public async Task<double> GetAverageRatingByCandidateIdAsync(Guid candidateId)
        {
            var average = await _context.Feedbacks
                .Where(f => f.CandidateId == candidateId)
                .Select(f => (double?)f.Rating)
                .AverageAsync();

            return average ?? 0;
        }
    }
}

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
            return await GetPagedFeedbacksByFilterAsync(page, pageSize, null);
        }

        public async Task<(IReadOnlyList<Feedback> Items, int TotalCount)> GetPagedFeedbacksByFilterAsync(int page, int pageSize, Guid? coachId = null)
        {
            var query = _context.Feedbacks.Include(f => f.CoachProfile).ThenInclude(cp => cp.User).Include(f => f.InterviewRoom).AsQueryable();

            if (coachId.HasValue)
            {
                query = query.Where(f => f.CoachId == coachId.Value);
            }

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

        public async Task<(double AverageRating, int TotalRatings)> GetAverageRatingByCoachIdAsync(Guid coachId)
        {
            var ratingsByRoom = await _context.Feedbacks
                .Where(f => f.CoachId == coachId)
                .GroupBy(f => f.InterviewRoomId)
                .Select(g => g.Max(f => f.Rating))
                .ToListAsync();

            if (ratingsByRoom.Count == 0)
            {
                return (0, 0);
            }

            var averageRating = ratingsByRoom.Average();
            return (averageRating, ratingsByRoom.Count);
        }

        public async Task<(double AverageRating, int TotalRatings)> GetAverageRatingByCandidateIdAsync(Guid candidateId)
        {
            var ratingsByRoom = await _context.Feedbacks
                .Where(f => f.CandidateId == candidateId)
                .GroupBy(f => f.InterviewRoomId)
                .Select(g => g.Max(f => f.Rating))
                .ToListAsync();

            if (ratingsByRoom.Count == 0)
            {
                return (0, 0);
            }

            var averageRating = ratingsByRoom.Average();
            return (averageRating, ratingsByRoom.Count);
        }

        public async Task<List<(Feedback Feedback, string CandidateName)>> GetRecentFeedbacksByCoachIdAsync(Guid coachId, int limit)
        {
            var results = await _context.Feedbacks
                .Where(f => f.CoachId == coachId)
                .Include(f => f.InterviewRoom)
                .Join(_context.Users,
                    f => f.CandidateId,
                    u => u.Id,
                    (f, u) => new { Feedback = f, CandidateName = u.FullName })
                .OrderByDescending(x => x.Feedback.InterviewRoom.ScheduledTime)
                .Take(limit)
                .ToListAsync();

            return results.Select(r => (r.Feedback, r.CandidateName)).ToList();
        }
    }
}

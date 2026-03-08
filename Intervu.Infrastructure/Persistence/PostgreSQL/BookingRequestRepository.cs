using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class BookingRequestRepository(IntervuPostgreDbContext context)
        : RepositoryBase<BookingRequest>(context), IBookingRequestRepository
    {
        public async Task<BookingRequest?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _context.BookingRequests
                .Include(br => br.Candidate)
                    .ThenInclude(c => c.User)
                .Include(br => br.Coach)
                    .ThenInclude(c => c.User)
                .Include(br => br.CoachInterviewService)
                    .ThenInclude(s => s!.InterviewType)
                .Include(br => br.Rounds.OrderBy(r => r.RoundNumber))
                    .ThenInclude(r => r.CoachInterviewService)
                        .ThenInclude(s => s.InterviewType)
                .Include(br => br.Transactions)
                .FirstOrDefaultAsync(br => br.Id == id);
        }

        public async Task<(IReadOnlyList<BookingRequest> Items, int TotalCount)> GetPagedByCandidateIdAsync(
            Guid candidateId, BookingRequestType? type, List<BookingRequestStatus>? statuses, int page, int pageSize)
        {
            var query = _context.BookingRequests
                .Include(br => br.Coach)
                    .ThenInclude(c => c.User)
                .Include(br => br.CoachInterviewService)
                    .ThenInclude(s => s!.InterviewType)
                .Include(br => br.Rounds.OrderBy(r => r.RoundNumber))
                .Where(br => br.CandidateId == candidateId);

            if (type.HasValue)
                query = query.Where(br => br.Type == type.Value);

            if (statuses != null && statuses.Count > 0)
                query = query.Where(br => statuses.Contains(br.Status));

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(br => br.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<(IReadOnlyList<BookingRequest> Items, int TotalCount)> GetPagedByCoachIdAsync(
            Guid coachId, BookingRequestType? type, List<BookingRequestStatus>? statuses, int page, int pageSize)
        {
            var query = _context.BookingRequests
                .Include(br => br.Candidate)
                    .ThenInclude(c => c.User)
                .Include(br => br.CoachInterviewService)
                    .ThenInclude(s => s!.InterviewType)
                .Include(br => br.Rounds.OrderBy(r => r.RoundNumber))
                .Where(br => br.CoachId == coachId);

            if (type.HasValue)
                query = query.Where(br => br.Type == type.Value);

            if (statuses != null && statuses.Count > 0)
                query = query.Where(br => statuses.Contains(br.Status));

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(br => br.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<IEnumerable<BookingRequest>> GetExpiredPendingRequestsAsync()
        {
            return await _context.BookingRequests
                .Where(br => br.Status == BookingRequestStatus.Pending
                    && br.ExpiresAt != null
                    && br.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task<List<(DateTime Start, DateTime End)>> GetActiveRoundsByCoachAsync(
            Guid coachId, DateTime rangeStart, DateTime rangeEnd)
        {
            var activeStatuses = new[]
            {
                BookingRequestStatus.Pending,
                BookingRequestStatus.Accepted,
                BookingRequestStatus.Paid
            };

            return await _context.BookingRequests
                .Where(br => br.CoachId == coachId && activeStatuses.Contains(br.Status))
                .SelectMany(br => br.Rounds)
                .Where(r => r.StartTime < rangeEnd && r.EndTime > rangeStart)
                .Select(r => new { r.StartTime, r.EndTime })
                .AsNoTracking()
                .ToListAsync()
                .ContinueWith(t => t.Result.Select(r => (r.StartTime, r.EndTime)).ToList());
        }
    }
}

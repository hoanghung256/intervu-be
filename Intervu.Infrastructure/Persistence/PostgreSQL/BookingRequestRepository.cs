using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class BookingRequestRepository(IntervuPostgreDbContext context)
        : RepositoryBase<BookingRequest>(context), IBookingRequestRepository
    {
        // Include a safety window on month boundaries so slots near midnight
        // are not dropped when clients render in non-UTC time zones.
        private const int MonthBoundaryTimezoneBufferHours = 14;

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
                .Include(br => br.Rounds.OrderBy(r => r.RoundNumber))
                    .ThenInclude(r => r.InterviewRoom)
                .Include(br => br.Rounds.OrderBy(r => r.RoundNumber))
                    .ThenInclude(r => r.AvailabilityBlocks)
                .Include(br => br.Transactions)
                .AsSplitQuery()
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
                    .ThenInclude(r => r.InterviewRoom)
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
                    .ThenInclude(r => r.InterviewRoom)
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
                .Include(br => br.Rounds)
                    .ThenInclude(r => r.AvailabilityBlocks)
                .AsSplitQuery()
                .Where(br => br.Status == BookingRequestStatus.Pending
                    && br.ExpiresAt != null
                    && br.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task<IEnumerable<BookingRequest>> GetExpiredPaidRequestsAsync()
        {
            return await _context.BookingRequests
                .Include(br => br.Candidate)
                    .ThenInclude(c => c.User)
                .Include(br => br.Coach)
                    .ThenInclude(c => c.User)
                .Include(br => br.Rounds)
                    .ThenInclude(r => r.AvailabilityBlocks)
                .Include(br => br.Transactions)
                .AsSplitQuery()
                .Where(br => br.Status == BookingRequestStatus.PendingForApprovalAfterPayment
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
                BookingRequestStatus.PendingForApprovalAfterPayment,
                BookingRequestStatus.Accepted
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

        public async Task<List<(DateTime Start, DateTime End)>> GetConfirmedBookingsForCoachAsync(Guid coachId, int month, int year)
        {
            var query = _context.BookingRequests
                .Where(br => br.CoachId == coachId)
                .Where(br => br.Status == BookingRequestStatus.Accepted || br.Status == BookingRequestStatus.PendingForApprovalAfterPayment)
                .SelectMany(br => br.Rounds);

            if (month > 0 && year > 0)
            {
                var monthStartUtc = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
                var monthEndUtc = monthStartUtc.AddMonths(1);
                var queryStartUtc = monthStartUtc.AddHours(-MonthBoundaryTimezoneBufferHours);
                var queryEndUtc = monthEndUtc.AddHours(MonthBoundaryTimezoneBufferHours);

                query = query.Where(r => r.StartTime >= queryStartUtc && r.StartTime < queryEndUtc);
            }

            return await query
                .Select(r => new ValueTuple<DateTime, DateTime>(r.StartTime, r.EndTime))
                .ToListAsync();
        }

        public async Task<List<InterviewRound>> GetConfirmedBookingEntitiesForCoachAsync(Guid coachId, int month, int year)
        {
            var query = _context.BookingRequests
                .Include(br => br.Candidate)
                    .ThenInclude(c => c.User)
                .Where(br => br.CoachId == coachId)
                .Where(br => br.Status == BookingRequestStatus.Accepted || br.Status == BookingRequestStatus.PendingForApprovalAfterPayment);

            // Wait, EF Core requires care with filtering included collections or we just filter the rounds
            var roundsQuery = query.SelectMany(br => br.Rounds);

            if (month > 0 && year > 0)
            {
                var monthStartUtc = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
                var monthEndUtc = monthStartUtc.AddMonths(1);
                var queryStartUtc = monthStartUtc.AddHours(-MonthBoundaryTimezoneBufferHours);
                var queryEndUtc = monthEndUtc.AddHours(MonthBoundaryTimezoneBufferHours);

                roundsQuery = roundsQuery.Where(r => r.StartTime >= queryStartUtc && r.StartTime < queryEndUtc);
            }

            return await roundsQuery
                .Include(r => r.BookingRequest)
                    .ThenInclude(br => br.Candidate)
                        .ThenInclude(c => c.User)
                .ToListAsync();
        }
    }
}

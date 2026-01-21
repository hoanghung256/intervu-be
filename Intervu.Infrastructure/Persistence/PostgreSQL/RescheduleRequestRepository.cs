using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class RescheduleRequestRepository : RepositoryBase<InterviewRescheduleRequest>, IRescheduleRequestRepository
    {
        public RescheduleRequestRepository(IntervuPostgreDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<InterviewRescheduleRequest>> GetExpiredRequestsAsync()
        {
            var requests = await _context.InterviewRescheduleRequests
                .Include(r => r.CurrentAvailability)
                .Include(r => r.ProposedAvailability)
                .Include(r => r.RequestedBy)
                .Where(r => r.Status == RescheduleRequestStatus.Pending &&
                    r.ExpiresAt <= DateTime.UtcNow)
                .ToListAsync();
            return requests;
        }

        public async Task<InterviewRescheduleRequest?> GetPendingRequestByBookingIdAsync(Guid bookingId)
        {
            var request = await _context.InterviewRescheduleRequests
                .Include(r => r.CurrentAvailability)
                .Include(r => r.ProposedAvailability)
                .Include(r => r.RequestedBy)
                .FirstOrDefaultAsync(r => r.InterviewBookingTransactionId == bookingId &&
                r.Status == RescheduleRequestStatus.Pending);
            return request;
        }

        public async Task<InterviewRescheduleRequest?> GetByIdWithDetailsAsync(Guid id)
        {
            var request = await _context.InterviewRescheduleRequests
                .Include(r => r.CurrentAvailability)
                .Include(r => r.ProposedAvailability)
                .Include(r => r.RequestedBy)
                .Include(r => r.RespondedBy)
                .Include(r => r.Booking)
                .FirstOrDefaultAsync(r => r.Id == id);
            return request;
        }

        public async Task<IEnumerable<InterviewRescheduleRequest>> GetPendingRequestsForResponderAsync(Guid responderId)
        {
            var requests = await _context.InterviewRescheduleRequests
                .Include(r => r.CurrentAvailability)
                .Include(r => r.ProposedAvailability)
                .Include(r => r.RequestedBy)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.CoachAvailability)
                .Where(r => r.Booking.CoachAvailability.CoachId == responderId &&
                    r.Status == RescheduleRequestStatus.Pending)
                .ToListAsync();
            return requests;
        }

        public async Task<bool> HasPendingRequestAsync(Guid bookingId)
        {
            return await _context.InterviewRescheduleRequests
                .AnyAsync(r => r.InterviewBookingTransactionId == bookingId &&
                    r.Status == RescheduleRequestStatus.Pending);
        }

        public async Task<IEnumerable<InterviewRescheduleRequest>> GetPendingRequestsByUserIdAsync(Guid userId)
        {
            var requests = await _context.InterviewRescheduleRequests
                .Include(r => r.CurrentAvailability)
                .Include(r => r.ProposedAvailability)
                .Include(r => r.RequestedBy)
                .Where(r => r.RequestedBy == userId &&
                    r.Status == RescheduleRequestStatus.Pending)
                .ToListAsync();
            return requests;
        }

        public async Task<IEnumerable<InterviewRescheduleRequest>> GetRequestsByBookingIdAsync(Guid bookingId)
        {
            var requests = await _context.InterviewRescheduleRequests
                .Include(r => r.CurrentAvailability)
                .Include(r => r.ProposedAvailability)
                .Include(r => r.RequestedBy)
                .Where(r => r.InterviewBookingTransactionId == bookingId)
                .ToListAsync();
            return requests;
        }
    }
}

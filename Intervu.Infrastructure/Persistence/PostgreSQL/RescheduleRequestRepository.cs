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
                .Include(r => r.Requester)
                .Where(r => r.Status == RescheduleRequestStatus.Pending &&
                    r.ExpiresAt <= DateTime.UtcNow)
                .ToListAsync();
            return requests;
        }

        public async Task<InterviewRescheduleRequest?> GetPendingRequestByRoomIdAsync(Guid roomId)
        {
            var request = await _context.InterviewRescheduleRequests
                .Include(r => r.CurrentAvailability)
                .Include(r => r.ProposedAvailability)
                .Include(r => r.Requester)
                .FirstOrDefaultAsync(r => r.InterviewRoomId == roomId &&
                r.Status == RescheduleRequestStatus.Pending);
            return request;
        }

        public async Task<InterviewRescheduleRequest?> GetByIdWithDetailsAsync(Guid id)
        {
            var request = await _context.InterviewRescheduleRequests
                .Include(r => r.CurrentAvailability)
                .Include(r => r.ProposedAvailability)
                .Include(r => r.Requester)
                .Include(r => r.Responder)
                .Include(r => r.InterviewRoom)
                .FirstOrDefaultAsync(r => r.Id == id);
            return request;
        }

        public async Task<IEnumerable<InterviewRescheduleRequest>> GetPendingRequestsForResponderAsync(Guid responderId)
        {
            var requests = await _context.InterviewRescheduleRequests
                .Include(r => r.CurrentAvailability)
                .Include(r => r.ProposedAvailability)
                .Include(r => r.Requester)
                .Include(r => r.InterviewRoom)
                .Where(r => r.InterviewRoom != null && 
                    (r.InterviewRoom.CoachId == responderId || r.InterviewRoom.CandidateId == responderId) &&
                    r.RequestedBy != responderId)
                .ToListAsync();
            return requests;
        }

        public async Task<bool> HasPendingRequestAsync(Guid roomId)
        {
            return await _context.InterviewRescheduleRequests
                .AnyAsync(r => r.InterviewRoomId == roomId &&
                    r.Status == RescheduleRequestStatus.Pending);
        }

        public async Task<IEnumerable<InterviewRescheduleRequest>> GetRescheduleRequestsByUserIdAsync(Guid userId)
        {
            // Get all requests where:
            // 1. User is the requester (RequestedBy == userId), OR
            // 2. User is the intended responder (they are Coach or Candidate of the room, but NOT the requester)
            var requests = await _context.InterviewRescheduleRequests
                .Include(r => r.CurrentAvailability)
                .Include(r => r.ProposedAvailability)
                .Include(r => r.Requester)
                .Include(r => r.Responder)
                .Include(r => r.InterviewRoom)
                .Where(r => 
                    // User sent the request
                    r.RequestedBy == userId || 
                    // User is the coach/candidate of the room (intended responder) but not the requester
                    (r.InterviewRoom != null && (r.InterviewRoom.CoachId == userId || r.InterviewRoom.CandidateId == userId)))
                .OrderByDescending(r => r.ExpiresAt)
                .ToListAsync();
            return requests;
        }

        public async Task<IEnumerable<InterviewRescheduleRequest>> GetRequestsByRoomIdAsync(Guid roomId)
        {
            var requests = await _context.InterviewRescheduleRequests
                .Include(r => r.CurrentAvailability)
                .Include(r => r.ProposedAvailability)
                .Include(r => r.Requester)
                .Where(r => r.InterviewRoomId == roomId)
                .ToListAsync();
            return requests;
        }
    }
}

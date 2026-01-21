using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Domain.Entities;

namespace Intervu.Domain.Repositories
{
    public interface IRescheduleRequestRepository : IRepositoryBase<InterviewRescheduleRequest>
    {
        Task<IEnumerable<InterviewRescheduleRequest>> GetRequestsByBookingIdAsync(Guid bookingId);
        Task<InterviewRescheduleRequest?> GetPendingRequestByBookingIdAsync(Guid bookingId);
        Task<InterviewRescheduleRequest?> GetByIdWithDetailsAsync(Guid id);
        Task<IEnumerable<InterviewRescheduleRequest>> GetExpiredRequestsAsync();
        Task<IEnumerable<InterviewRescheduleRequest>> GetPendingRequestsByUserIdAsync(Guid userId);
        Task<IEnumerable<InterviewRescheduleRequest>> GetPendingRequestsForResponderAsync(Guid responderId);
        Task<bool> HasPendingRequestAsync(Guid bookingId);
    }
}

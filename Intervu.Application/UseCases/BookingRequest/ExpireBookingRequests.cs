using Intervu.Application.Interfaces.UseCases.BookingRequest;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.BookingRequest
{
    internal class ExpireBookingRequests : IExpireBookingRequests
    {
        private readonly IBookingRequestRepository _bookingRepo;

        public ExpireBookingRequests(IBookingRequestRepository bookingRepo)
        {
            _bookingRepo = bookingRepo;
        }

        public async Task<int> ExecuteAsync()
        {
            var expiredRequests = (await _bookingRepo.GetExpiredPendingRequestsAsync()).ToList();

            if (expiredRequests.Count == 0)
                return 0;

            foreach (var request in expiredRequests)
            {
                request.Status = BookingRequestStatus.Expired;
                request.UpdatedAt = DateTime.UtcNow;
                _bookingRepo.UpdateAsync(request);
            }

            await _bookingRepo.SaveChangesAsync();

            return expiredRequests.Count;
        }
    }
}

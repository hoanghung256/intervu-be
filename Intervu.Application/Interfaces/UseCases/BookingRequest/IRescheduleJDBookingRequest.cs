using Intervu.Application.DTOs.BookingRequest;

namespace Intervu.Application.Interfaces.UseCases.BookingRequest
{
    public interface IRescheduleJDBookingRequest
    {
        Task ExecuteAsync(Guid candidateId, Guid bookingRequestId, RescheduleJDBookingRequestDto dto);
    }
}

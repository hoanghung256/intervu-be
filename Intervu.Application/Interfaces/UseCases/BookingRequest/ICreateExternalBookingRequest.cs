using Intervu.Application.DTOs.BookingRequest;

namespace Intervu.Application.Interfaces.UseCases.BookingRequest
{
    public interface ICreateExternalBookingRequest
    {
        /// <summary>
        /// Flow B: Candidate requests a session outside the coach's available time ranges.
        /// Creates a BookingRequest with Pending status and ExpiresAt set.
        /// </summary>
        Task<BookingRequestDto> ExecuteAsync(Guid candidateId, CreateExternalBookingRequestDto dto);
    }
}

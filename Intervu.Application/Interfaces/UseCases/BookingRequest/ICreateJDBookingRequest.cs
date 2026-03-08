using Intervu.Application.DTOs.BookingRequest;

namespace Intervu.Application.Interfaces.UseCases.BookingRequest
{
    public interface ICreateJDBookingRequest
    {
        /// <summary>
        /// Flow C: Candidate submits JD + CV for a multi-round interview plan.
        /// Creates a BookingRequest with Rounds, Pending status, and ExpiresAt set.
        /// </summary>
        Task<BookingRequestDto> ExecuteAsync(Guid candidateId, CreateJDBookingRequestDto dto);
    }
}

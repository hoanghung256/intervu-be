using Intervu.Application.DTOs.BookingRequest;

namespace Intervu.Application.Interfaces.UseCases.BookingRequest
{
    public interface IRespondToBookingRequest
    {
        /// <summary>
        /// Coach accepts or rejects a pending booking request.
        /// On accept: Status → Accepted, RespondedAt set.
        /// On reject: Status → Rejected, RejectionReason set, RespondedAt set.
        /// </summary>
        Task<BookingRequestDto> ExecuteAsync(Guid coachId, Guid bookingRequestId, RespondToBookingRequestDto dto);
    }
}

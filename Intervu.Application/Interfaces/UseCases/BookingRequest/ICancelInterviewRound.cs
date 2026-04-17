using Intervu.Application.DTOs.BookingRequest;

namespace Intervu.Application.Interfaces.UseCases.BookingRequest
{
    public interface ICancelInterviewRound
    {
        // Cancel a specific round within an accepted booking request.
        // This allows candidates to cancel individual rounds without cancelling the entire booking.
        // But when its the last remaining round, cancelling the whole booking request.
        Task<BookingRequestDto> ExecuteAsync(Guid candidateId, Guid bookingRequestId, Guid roundId);
    }
}

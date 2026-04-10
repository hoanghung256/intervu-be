using Intervu.Application.DTOs.BookingRequest;

namespace Intervu.Application.Interfaces.UseCases.BookingRequest
{
    public interface ICancelInterviewRound
    {
        Task<BookingRequestDto> ExecuteAsync(Guid candidateId, Guid bookingRequestId, Guid roundId);
    }
}

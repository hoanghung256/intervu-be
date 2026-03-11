using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.InterviewBooking;

namespace Intervu.Application.Interfaces.UseCases.InterviewBooking
{
    public interface IGetInterviewBookingHistory
    {
        Task<PagedResult<InterviewBookingTransactionHistoryDto>> ExecuteAsync(Guid userId, GetInterviewBookingHistoryRequest request);
    }
}

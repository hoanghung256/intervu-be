using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.Interfaces.UseCases.InterviewBooking
{
    public interface IUpdateBookingStatus
    {
        Task<InterviewBookingTransaction> ExecuteAsync(Guid bookingId, TransactionStatus transactionStatus);
    }
}

using Intervu.Domain.Entities;

namespace Intervu.Application.Interfaces.UseCases.InterviewBooking
{
    public interface IProcessBookingPayment
    {
        Task ExecuteAsync(InterviewBookingTransaction transaction);
    }
}

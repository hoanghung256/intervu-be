using System;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.InterviewBooking
{
    public interface IRefundForCandidate
    {
        Task ExecuteAsync(Guid bookingRequestId);
    }
}

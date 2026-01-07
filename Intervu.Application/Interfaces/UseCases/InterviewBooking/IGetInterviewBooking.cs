using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.InterviewBooking
{
    public interface IGetInterviewBooking
    {
        Task<Domain.Entities.InterviewBookingTransaction?> ExecuteAsync(Guid interviewBookingId);
    }
}

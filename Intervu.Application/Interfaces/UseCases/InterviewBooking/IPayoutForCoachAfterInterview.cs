using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.InterviewBooking
{
    public interface IPayoutForCoachAfterInterview
    {
        Task ExecuteAsync(Guid interviewRoomId);
    }
}

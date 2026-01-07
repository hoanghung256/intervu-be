using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.InterviewBooking
{
    public interface IPayoutForInterviewerAfterInterview
    {
        Task ExecuteAsync(Guid interviewRoomId);
    }
}

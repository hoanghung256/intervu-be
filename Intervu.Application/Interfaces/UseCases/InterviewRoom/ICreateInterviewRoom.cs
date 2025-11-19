using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.InterviewRoom
{
    public interface ICreateInterviewRoom
    {
        Task<int> ExecuteAsync(int interveweeId);
        Task<int> ExecuteAsync(int interveweeId, int interviewerId, DateTime scheduledTime);
    }
}

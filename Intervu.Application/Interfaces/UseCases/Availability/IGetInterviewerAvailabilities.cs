using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Domain.Entities;

namespace Intervu.Application.Interfaces.UseCases.Availability
{
    public interface IGetInterviewerAvailabilities
    {
        Task<IEnumerable<InterviewerAvailability>> ExecuteAsync(int intervewerId, int month = 0, int year = 0);

        Task<InterviewerAvailability> GetAsync(int interviewerId, DateTime startTime);
    }
}

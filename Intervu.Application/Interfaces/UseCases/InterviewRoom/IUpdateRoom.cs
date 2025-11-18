using Intervu.Domain.Entities.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.InterviewRoom
{
    public interface IUpdateRoom
    {
        Task ExecuteAsync(Domain.Entities.InterviewRoom interviewRoom);
    }
}

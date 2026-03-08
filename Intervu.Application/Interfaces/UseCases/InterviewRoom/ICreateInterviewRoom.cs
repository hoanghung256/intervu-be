using Intervu.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.InterviewRoom
{
    public interface ICreateInterviewRoom
    {
        Task<Guid> ExecuteAsync(Guid interveweeId);
        Task<Guid> ExecuteAsync(Guid interveweeId, Guid interviewerId, Guid availabilityId, DateTime startTime, Guid transactionId, int duration);
    }
}

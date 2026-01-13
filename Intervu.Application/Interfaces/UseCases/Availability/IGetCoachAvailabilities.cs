using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Domain.Entities;

namespace Intervu.Application.Interfaces.UseCases.Availability
{
    public interface IGetCoachAvailabilities
    {
        Task<IEnumerable<CoachAvailability>> ExecuteAsync(Guid coachId, int month = 0, int year = 0);

        Task<CoachAvailability?> GetAsync(Guid coachId, DateTime startTime);
    }
}

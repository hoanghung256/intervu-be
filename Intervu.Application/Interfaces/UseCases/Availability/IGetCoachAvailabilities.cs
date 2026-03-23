using Intervu.Application.DTOs.Availability;
using Intervu.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Availability
{
    public interface IGetCoachAvailabilities
    {
        Task<CoachScheduleDto> ExecuteAsync(Guid coachId, int month = 0, int year = 0);
        
        Task<CoachAvailability?> GetAsync(Guid coachId, DateTime startTime);
    }
}

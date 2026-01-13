using Intervu.Application.Interfaces.UseCases.Availability;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Availability
{
    public class UpdateAvailabilityStatus : IUpdateAvailabilityStatus
    {
        private readonly ICoachAvailabilitiesRepository _coachAvailabilitiesRepository;

        public UpdateAvailabilityStatus(ICoachAvailabilitiesRepository coachAvailabilitiesRepository) 
        {
            _coachAvailabilitiesRepository = coachAvailabilitiesRepository;
        }

        public async Task<CoachAvailability> ExecuteAsync(Guid availabilityId, bool isBooked)
        {
            var a = await _coachAvailabilitiesRepository.GetByIdAsync(availabilityId) ?? throw new Exception("CoachAvailability not found");

            a.IsBooked = isBooked;
            _coachAvailabilitiesRepository.UpdateAsync(a);
            await _coachAvailabilitiesRepository.SaveChangesAsync();
            return a;
        }
    }
}

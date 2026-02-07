using Intervu.Application.Interfaces.UseCases.Availability;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
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

        public async Task<CoachAvailability> ExecuteAsync(Guid availabilityId, CoachAvailabilityStatus status)
        {
            var a = await _coachAvailabilitiesRepository.GetByIdAsync(availabilityId) ?? throw new Exception("CoachAvailability not found");

            a.Status = status;

            await _coachAvailabilitiesRepository.SaveChangesAsync();
            return a;
        }
    }
}

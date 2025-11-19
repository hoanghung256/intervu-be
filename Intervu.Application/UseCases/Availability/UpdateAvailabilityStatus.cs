using Intervu.Application.Interfaces.Repositories;
using Intervu.Application.Interfaces.UseCases.Availability;
using Intervu.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Availability
{
    public class UpdateAvailabilityStatus : IUpdateAvailabilityStatus
    {
        private readonly IInterviewerAvailabilitiesRepository _interviewerAvailabilitiesRepository;

        public UpdateAvailabilityStatus(IInterviewerAvailabilitiesRepository interviewerAvailabilitiesRepository) 
        {
            _interviewerAvailabilitiesRepository = interviewerAvailabilitiesRepository;
        }

        public async Task<InterviewerAvailability> ExecuteAsync(int availabilityId, bool isBooked)
        {
            var a = await _interviewerAvailabilitiesRepository.GetByIdAsync(availabilityId) ?? throw new Exception("InterviewerAvailability not found");

            a.IsBooked = isBooked;
            _interviewerAvailabilitiesRepository.UpdateAsync(a);
            await _interviewerAvailabilitiesRepository.SaveChangesAsync();

            return a;
        }
    }
}

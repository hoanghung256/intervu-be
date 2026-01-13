using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Application.Interfaces.UseCases.Availability;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.Availability
{
    public class GetCoachAvailabilities : IGetCoachAvailabilities
    {
        private readonly ICoachAvailabilitiesRepository _coachAvailabilitiesRepository;
        public GetCoachAvailabilities(ICoachAvailabilitiesRepository coachAvailabilitiesRepository)
        {
            _coachAvailabilitiesRepository = coachAvailabilitiesRepository;
        }
        public Task<IEnumerable<CoachAvailability>> ExecuteAsync(Guid coachId, int month = 0, int year = 0)
        {
            return _coachAvailabilitiesRepository.GetCoachAvailabilitiesByMonthAsync(coachId, month, year);
        }

        public Task<CoachAvailability?> GetAsync(Guid coachId, DateTime startTime)
        {
            return _coachAvailabilitiesRepository.GetAsync(coachId, startTime);
        }
    }
}

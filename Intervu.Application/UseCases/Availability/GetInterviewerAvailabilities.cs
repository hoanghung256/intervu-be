using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Application.Interfaces.Repositories;
using Intervu.Application.Interfaces.UseCases.Availability;
using Intervu.Domain.Entities;

namespace Intervu.Application.UseCases.Availability
{
    public class GetInterviewerAvailabilities : IGetInterviewerAvailabilities
    {
        private readonly IInterviewerAvailabilitiesRepository _interviewerAvailabilitiesRepository;
        public GetInterviewerAvailabilities(IInterviewerAvailabilitiesRepository interviewerAvailabilitiesRepository)
        {
            _interviewerAvailabilitiesRepository = interviewerAvailabilitiesRepository;
        }
        public Task<IEnumerable<InterviewerAvailability>> ExecuteAsync(int intervewerId, int month = 0, int year = 0)
        {
            return _interviewerAvailabilitiesRepository.GetInterviewerAvailabilitiesByMonthAsync(intervewerId, month, year);
        }
    }
}

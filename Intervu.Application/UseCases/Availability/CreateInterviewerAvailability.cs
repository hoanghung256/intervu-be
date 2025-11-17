using System;
using System.Threading.Tasks;
using AutoMapper;
using Intervu.Application.DTOs.Availability;
using Intervu.Application.Interfaces.Repositories;
using Intervu.Application.Interfaces.UseCases.Availability;
using Intervu.Domain.Entities;

namespace Intervu.Application.UseCases.Availability
{
    public class CreateInterviewerAvailability : ICreateInterviewerAvailability
    {
        private readonly IInterviewerAvailabilitiesRepository _repo;
        private readonly IMapper _mapper;

        public CreateInterviewerAvailability(IInterviewerAvailabilitiesRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<int> ExecuteAsync(InterviewerAvailabilityCreateDto dto)
        {
            // basic validation
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (dto.EndTime <= dto.StartTime) throw new ArgumentException("EndTime must be greater than StartTime");

            // prevent creating in the past
            if (dto.EndTime <= DateTime.UtcNow) throw new ArgumentException("Cannot create availability in the past");

            // check overlap
            var startOffset = new DateTimeOffset(dto.StartTime);
            var endOffset = new DateTimeOffset(dto.EndTime);
            bool isAvailable = await _repo.IsInterviewerAvailableAsync(dto.InterviewerId, startOffset, endOffset);
            if (!isAvailable) throw new InvalidOperationException("Interviewer has overlapping availability or existing booking");

            var entity = _mapper.Map<InterviewerAvailability>(dto);
            var id = await _repo.CreateInterviewerAvailabilityAsync(entity);
            return id;
        }
    }
}
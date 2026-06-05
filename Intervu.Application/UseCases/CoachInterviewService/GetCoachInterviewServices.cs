using AutoMapper;
using Intervu.Application.DTOs.CoachInterviewService;
using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.UseCases.CoachInterviewService;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.CoachInterviewService
{
    internal class GetCoachInterviewServices : IGetCoachInterviewServices
    {
        private readonly ICoachInterviewServiceRepository _serviceRepo;
        private readonly ICoachProfileRepository _coachRepo;
        private readonly IMapper _mapper;

        public GetCoachInterviewServices(
            ICoachInterviewServiceRepository serviceRepo,
            ICoachProfileRepository coachRepo,
            IMapper mapper)
        {
            _serviceRepo = serviceRepo;
            _coachRepo = coachRepo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CoachInterviewServiceDto>> ExecuteAsync(Guid coachId, bool includeUnavailableForCoach = false)
        {
            if (!includeUnavailableForCoach)
            {
                _ = await _coachRepo.GetProfileByIdAsync(coachId)
                    ?? throw new NotFoundException($"Coach profile with ID {coachId} not found.");
            }

            var services = (await _serviceRepo.GetByCoachIdAsync(coachId)).ToList();

            if (includeUnavailableForCoach)
            {
                if (services.Count == 0)
                    throw new NotFoundException($"No interview services found for coach with ID {coachId}");
                return _mapper.Map<IEnumerable<CoachInterviewServiceDto>>(services);
            }

            var bookable = services.Where(s => s.InterviewType.Status == InterviewTypeStatus.Active).ToList();
            return _mapper.Map<IEnumerable<CoachInterviewServiceDto>>(bookable);
        }
    }
}

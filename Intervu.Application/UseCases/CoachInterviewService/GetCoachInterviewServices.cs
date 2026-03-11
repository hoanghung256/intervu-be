using AutoMapper;
using Intervu.Application.DTOs.CoachInterviewService;
using Intervu.Application.Interfaces.UseCases.CoachInterviewService;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.CoachInterviewService
{
    internal class GetCoachInterviewServices : IGetCoachInterviewServices
    {
        private readonly ICoachInterviewServiceRepository _serviceRepo;
        private readonly IMapper _mapper;

        public GetCoachInterviewServices(ICoachInterviewServiceRepository serviceRepo, IMapper mapper)
        {
            _serviceRepo = serviceRepo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CoachInterviewServiceDto>> ExecuteAsync(Guid coachId)
        {
            var services = await _serviceRepo.GetByCoachIdAsync(coachId);
            return _mapper.Map<IEnumerable<CoachInterviewServiceDto>>(services);
        }
    }
}

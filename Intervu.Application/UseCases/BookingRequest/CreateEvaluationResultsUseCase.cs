using Intervu.Application.Interfaces.UseCases.BookingRequest;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using System.Linq;

namespace Intervu.Application.UseCases.BookingRequest
{
    internal class CreateEvaluationResultsUseCase : ICreateEvaluationResultsUseCase
    {
        private readonly ICoachInterviewServiceRepository _serviceRepo;

        public CreateEvaluationResultsUseCase(ICoachInterviewServiceRepository serviceRepo)
        {
            _serviceRepo = serviceRepo;
        }

        public async Task<List<EvaluationResult>> ExecuteAsync(Guid? coachInterviewServiceId)
        {
            if (coachInterviewServiceId == null) return new List<EvaluationResult>();

            var service = await _serviceRepo.GetByIdWithDetailsAsync(coachInterviewServiceId.Value);
            if (service == null) return new List<EvaluationResult>();

            return service.InterviewType?.EvaluationStructure
                   ?.Select(c => new EvaluationResult
                   {
                       Type = c.Type,
                       Question = c.Question,
                       Score = 0,
                       Answer = string.Empty
                   })
                   .ToList()
                   ?? new List<EvaluationResult>();
        }
    }
}

using Intervu.Domain.Entities;

namespace Intervu.Application.Interfaces.UseCases.BookingRequest
{
    public interface ICreateEvaluationResultsUseCase
    {
        Task<List<EvaluationResult>> ExecuteAsync(Guid? coachInterviewServiceId);
    }
}

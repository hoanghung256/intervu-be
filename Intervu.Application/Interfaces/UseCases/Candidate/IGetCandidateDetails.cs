using Intervu.Domain.Entities;

namespace Intervu.Application.Interfaces.UseCases.Candidate
{
    public interface IGetCandidateDetails
    {
        Task<User> ExecuteAsync(Guid candidateId);
    }
}

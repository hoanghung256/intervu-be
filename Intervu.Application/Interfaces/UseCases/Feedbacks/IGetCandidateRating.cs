using System;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Feedbacks
{
    public interface IGetCandidateRating
    {
        Task<double> ExecuteAsync(Guid candidateId);
    }
}

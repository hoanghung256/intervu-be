using Intervu.Application.Interfaces.UseCases.Feedbacks;
using Intervu.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Feedbacks
{
    public class GetCandidateRating : IGetCandidateRating
    {
        private readonly IFeedbackRepository _repository;

        public GetCandidateRating(IFeedbackRepository repository)
        {
            _repository = repository;
        }

        public Task<(double AverageRating, int TotalRatings)> ExecuteAsync(Guid candidateId)
        {
            return _repository.GetAverageRatingByCandidateIdAsync(candidateId);
        }
    }
}

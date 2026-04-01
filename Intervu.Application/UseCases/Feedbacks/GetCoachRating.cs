using Intervu.Application.Interfaces.UseCases.Feedbacks;
using Intervu.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Feedbacks
{
    public class GetCoachRating : IGetCoachRating
    {
        private readonly IFeedbackRepository _repository;

        public GetCoachRating(IFeedbackRepository repository)
        {
            _repository = repository;
        }

        public Task<double> ExecuteAsync(Guid coachId)
        {
            return _repository.GetAverageRatingByCoachIdAsync(coachId);
        }
    }
}

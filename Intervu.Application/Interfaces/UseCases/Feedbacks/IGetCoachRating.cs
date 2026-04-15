using System;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Feedbacks
{
    public interface IGetCoachRating
    {
        Task<(double AverageRating, int TotalRatings)> ExecuteAsync(Guid coachId);
    }
}

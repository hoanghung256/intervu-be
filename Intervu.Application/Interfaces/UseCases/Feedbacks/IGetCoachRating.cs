using System;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Feedbacks
{
    public interface IGetCoachRating
    {
        Task<double> ExecuteAsync(Guid coachId);
    }
}

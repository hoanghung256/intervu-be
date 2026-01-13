using Intervu.Domain.Entities;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Coach
{
    public interface IGetCoachDetails
    {
        Task<User> ExecuteAsync(Guid coachId);
    }
}

using Intervu.Application.Common;
using Intervu.Domain.Entities;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.Repositories
{
    public interface IFeedbackRepository : IRepositoryBase<Feedback>
    {
        Task<PagedResult<Feedback>> GetPagedFeedbacksAsync(int page, int pageSize);
        Task<int> GetTotalFeedbacksCountAsync();
        Task<double> GetAverageRatingAsync();
    }
}

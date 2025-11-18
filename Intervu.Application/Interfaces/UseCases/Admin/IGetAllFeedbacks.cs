using Intervu.Application.Common;
using Intervu.Application.DTOs.Admin;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Admin
{
    public interface IGetAllFeedbacks
    {
        Task<PagedResult<FeedbackDto>> ExecuteAsync(int page, int pageSize);
    }
}

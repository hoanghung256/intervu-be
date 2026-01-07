using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.Admin;

namespace Intervu.Application.Interfaces.UseCases.Admin
{
    public interface IGetAllFeedbacks
    {
        Task<PagedResult<FeedbackDto>> ExecuteAsync(int page, int pageSize);
    }
}

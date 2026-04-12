using System.Threading.Tasks;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.Tag;

namespace Intervu.Application.Interfaces.UseCases.Tag
{
    public interface IGetAllTags
    {
        Task<PagedResult<TagDto>> ExecuteAsync(int page, int pageSize);
    }
}

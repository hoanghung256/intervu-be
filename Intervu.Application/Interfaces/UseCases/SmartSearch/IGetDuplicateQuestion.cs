using Intervu.Application.DTOs.SmartSearch;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.SmartSearch
{
    public interface IGetDuplicateQuestion
    {
        Task<List<QuestionSmartSearchResultDto>> ExecuteAsync(QuestionSmartSearchRequestDto request);
    }
}

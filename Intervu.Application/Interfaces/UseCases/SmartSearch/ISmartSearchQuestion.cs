using Intervu.Application.DTOs.SmartSearch;

namespace Intervu.Application.Interfaces.UseCases.SmartSearch
{
    public interface ISmartSearchQuestion
    {
        Task<List<QuestionSmartSearchResultDto>> ExecuteAsync(QuestionSmartSearchRequestDto request);
    }
}

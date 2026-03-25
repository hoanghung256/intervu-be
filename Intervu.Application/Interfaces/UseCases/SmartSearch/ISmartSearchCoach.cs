using Intervu.Application.DTOs.SmartSearch;

namespace Intervu.Application.Interfaces.UseCases.SmartSearch
{
    public interface ISmartSearchCoach
    {
        Task<List<SmartSearchResultDto>> ExecuteAsync(SmartSearchRequest request);
    }
}

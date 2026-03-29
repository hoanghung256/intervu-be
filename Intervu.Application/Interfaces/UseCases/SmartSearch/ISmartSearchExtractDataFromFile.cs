using System.Threading.Tasks;
using Intervu.Application.DTOs.SmartSearch;

namespace Intervu.Application.Interfaces.UseCases.SmartSearch
{
    public interface ISmartSearchExtractDataFromFile
    {
        Task<string> ExecuteAsync(SmartSearchExtractRequestDto request);
    }
}

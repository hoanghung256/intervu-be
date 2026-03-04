using Intervu.Application.DTOs.Question;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Question
{
    public interface ISearchQuestions
    {
        Task<List<QuestionSearchResultDto>> ExecuteAsync(string keyword, int limit = 10);
    }
}

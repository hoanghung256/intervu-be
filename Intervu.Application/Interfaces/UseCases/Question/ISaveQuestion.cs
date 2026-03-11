using System;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Question
{
    public interface ISaveQuestion
    {
        Task<bool> ExecuteAsync(Guid questionId, bool isSaveQuestion, Guid userId);
    }
}

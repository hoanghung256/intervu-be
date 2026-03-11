using System;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Question
{
    public interface IDeleteQuestion
    {
        Task ExecuteAsync(Guid questionId, Guid userId);
    }
}

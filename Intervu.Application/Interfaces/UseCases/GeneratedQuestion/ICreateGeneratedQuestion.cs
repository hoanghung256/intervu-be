using Intervu.Application.DTOs.GeneratedQuestion;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.GeneratedQuestion
{
    public interface ICreateGeneratedQuestion
    {
        Task<Guid> ExecuteAsync(CreateGeneratedQuestionRequest request, Guid creatorUserId);
    }
}

using Intervu.Application.DTOs.Question;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Question
{
    public interface IAddQuestion
    {
        Task<AddQuestionResult> ExecuteAsync(Guid experienceId, CreateQuestionRequest request, Guid userId);
    }
}

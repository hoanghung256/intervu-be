using Intervu.Application.DTOs.Question;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Question
{
    public interface IGetQuestionDetail
    {
        Task<QuestionDetailDto?> ExecuteAsync(Guid questionId);
    }
}

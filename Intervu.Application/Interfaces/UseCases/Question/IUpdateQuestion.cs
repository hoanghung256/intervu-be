using Intervu.Application.DTOs.Question;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Question
{
    public interface IUpdateQuestion
    {
        Task ExecuteAsync(Guid questionId, UpdateQuestionRequest request, Guid userId);
        Task VoteAsync(Guid questionId, bool isUpvote, Guid userId);
    }
}

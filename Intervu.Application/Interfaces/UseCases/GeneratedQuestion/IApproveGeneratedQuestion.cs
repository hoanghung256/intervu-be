using Intervu.Application.DTOs.GeneratedQuestion;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.GeneratedQuestion
{
    public interface IApproveGeneratedQuestion
    {
        Task<Guid> ExecuteAsync(Guid generatedQuestionId, ApproveGeneratedQuestionRequest request, Guid reviewerUserId);
    }
}

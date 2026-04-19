using Intervu.Domain.Entities.Constants.QuestionConstants;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Question
{
    public interface IModerateQuestion
    {
        Task ExecuteAsync(Guid questionId, QuestionStatus newStatus, Guid adminUserId);
    }
}

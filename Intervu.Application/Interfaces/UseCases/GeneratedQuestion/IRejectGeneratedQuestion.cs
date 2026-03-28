using System;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.GeneratedQuestion
{
    public interface IRejectGeneratedQuestion
    {
        Task ExecuteAsync(Guid generatedQuestionId, Guid reviewerUserId);
    }
}

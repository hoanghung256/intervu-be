using Intervu.Application.DTOs.Ai;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.GeneratedQuestion
{
    public interface IStoreGeneratedQuestions
    {
        Task<int> ExecuteAsync(Guid interviewRoomId, IEnumerable<AiQuestionDto> questions);
    }
}

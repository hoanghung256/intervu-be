using Intervu.Application.DTOs.GeneratedQuestion;
using Intervu.Domain.Entities.Constants;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.GeneratedQuestion
{
    public interface IGetGeneratedQuestionsByRoom
    {
        Task<List<GeneratedQuestionDto>> ExecuteAsync(Guid interviewRoomId, GeneratedQuestionStatus? status);
    }
}

using Intervu.Application.DTOs.Question;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Question
{
    public interface IGetSavedQuestions
    {
        Task<List<QuestionListItemDto>> ExecuteAsync(Guid userId);
    }
}

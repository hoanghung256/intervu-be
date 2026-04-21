using Intervu.Application.DTOs.PreparedQuestion;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.PreparedQuestion
{
    public interface IGetPreparedQuestions
    {
        Task<List<PreparedQuestionDto>> ExecuteAsync(Guid interviewRoomId, Guid userId);
    }

    public interface IAddCustomPreparedQuestion
    {
        Task<PreparedQuestionDto> ExecuteAsync(
            Guid interviewRoomId,
            CreateCustomPreparedQuestionRequest request,
            Guid userId);
    }

    public interface IAddPreparedQuestionFromBank
    {
        Task<PreparedQuestionDto> ExecuteAsync(
            Guid interviewRoomId,
            ImportBankQuestionRequest request,
            Guid userId);
    }

    public interface IUpdatePreparedQuestion
    {
        Task<PreparedQuestionDto> ExecuteAsync(
            Guid preparedQuestionId,
            UpdatePreparedQuestionRequest request,
            Guid userId);
    }

    public interface IDeletePreparedQuestion
    {
        Task ExecuteAsync(Guid preparedQuestionId, Guid userId);
    }

    public interface IReorderPreparedQuestions
    {
        Task ExecuteAsync(
            Guid interviewRoomId,
            ReorderPreparedQuestionsRequest request,
            Guid userId);
    }

    public interface IMarkPreparedQuestionAsked
    {
        Task<PreparedQuestionDto> ExecuteAsync(Guid preparedQuestionId, Guid userId);
    }

    public interface IUnmarkPreparedQuestionAsked
    {
        Task<PreparedQuestionDto> ExecuteAsync(Guid preparedQuestionId, Guid userId);
    }

    public interface ISendPreparedQuestionToEditor
    {
        Task<PreparedQuestionDto> ExecuteAsync(Guid preparedQuestionId, Guid userId);
    }
}

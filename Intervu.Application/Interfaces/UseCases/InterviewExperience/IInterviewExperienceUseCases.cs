using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.InterviewExperience;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.InterviewExperience
{
    public interface IGetInterviewExperiences
    {
        Task<PagedResult<InterviewExperienceSummaryDto>> ExecuteAsync(InterviewExperienceFilterRequest filter);
    }

    public interface IGetInterviewExperienceDetail
    {
        Task<InterviewExperienceDetailDto?> ExecuteAsync(Guid id);
    }

    public interface ICreateInterviewExperience
    {
        Task<Guid> ExecuteAsync(CreateInterviewExperienceRequest request, Guid userId);
    }

    public interface IUpdateInterviewExperience
    {
        Task ExecuteAsync(Guid id, UpdateInterviewExperienceRequest request, Guid userId);
    }

    public interface IDeleteInterviewExperience
    {
        Task ExecuteAsync(Guid id, Guid userId);
    }

    public interface IAddQuestion
    {
        Task<Guid> ExecuteAsync(Guid experienceId, CreateQuestionRequest request, Guid userId);
    }

    public interface IUpdateQuestion
    {
        Task ExecuteAsync(Guid questionId, UpdateQuestionRequest request, Guid userId);
    }

    public interface IDeleteQuestion
    {
        Task ExecuteAsync(Guid questionId, Guid userId);
    }

    public interface IGetQuestionList
    {
        Task<PagedResult<QuestionListItemDto>> ExecuteAsync(QuestionFilterRequest filter);
    }
}

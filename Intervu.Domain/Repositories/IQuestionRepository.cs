using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Entities.Constants.QuestionConstants;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intervu.Domain.Repositories
{
    public interface IQuestionRepository : IRepositoryBase<Question>
    {
        Task<IEnumerable<Question>> GetByExperienceIdAsync(Guid interviewExperienceId);

        Task<(List<Question> Items, int TotalCount)> GetPagedAsync(
            string? searchTerm,
            Guid? companyId,
            Guid? tagId,
            QuestionCategory? category,
            Role? role,
            ExperienceLevel? level,
            InterviewRound? round,
            SortOption? sortBy,
            int page,
            int pageSize);

        Task<List<Question>> SearchAsync(string keyword, int limit = 10);

        Task<Question?> GetDetailAsync(Guid id);

        Task<List<Question>> GetRelatedAsync(Guid excludeId, Guid questionId, int limit);

        Task IncrementViewCountAsync(Guid questionId);
    }
}

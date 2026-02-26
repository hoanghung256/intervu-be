using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intervu.Domain.Repositories
{
    public interface IQuestionRepository : IRepositoryBase<Question>
    {
        /// <summary>Returns all questions belonging to a specific InterviewExperience.</summary>
        Task<IEnumerable<Question>> GetByExperienceIdAsync(Guid interviewExperienceId);

        /// <summary>Returns paginated questions with optional filters by type, role and level.</summary>
        Task<(List<Question> Items, int TotalCount)> GetPagedAsync(
            string? searchTerm,
            string? questionType,
            string? role,
            ExperienceLevel? level,
            int page,
            int pageSize);
    }
}

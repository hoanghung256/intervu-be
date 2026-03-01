using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
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
            string? questionType,
            string? role,
            ExperienceLevel? level,
            int page,
            int pageSize);

        Task<Question?> GetDetailAsync(Guid id);

        Task<List<Question>> GetRelatedAsync(Guid excludeId, string questionType, string role, int limit);
    }
}

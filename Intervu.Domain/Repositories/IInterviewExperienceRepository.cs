using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Entities.Constants.QuestionConstants;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intervu.Domain.Repositories
{
    public interface IInterviewExperienceRepository : IRepositoryBase<InterviewExperience>
    {
        Task<(List<InterviewExperience> Items, int TotalCount)> GetPagedAsync(
            string? searchTerm, Guid? company, Role? role, ExperienceLevel? level,
            Intervu.Domain.Entities.Constants.QuestionConstants.InterviewRound? lastRoundCompleted, SortOption? sortBy, int page, int pageSize);

        Task<InterviewExperience?> GetDetailAsync(Guid id);
    }
}

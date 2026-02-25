using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intervu.Domain.Repositories
{
    public interface IInterviewExperienceRepository : IRepositoryBase<InterviewExperience>
    {
        Task<(List<InterviewExperience> Items, int TotalCount)> GetPagedAsync(
            string? searchTerm, string? role, ExperienceLevel? level, string? lastRoundCompleted, int page, int pageSize);

        Task<InterviewExperience?> GetDetailAsync(Guid id);
    }
}

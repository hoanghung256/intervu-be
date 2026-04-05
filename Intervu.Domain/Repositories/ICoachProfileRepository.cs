using Intervu.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intervu.Domain.Repositories
{
    public interface ICoachProfileRepository : IRepositoryBase<CoachProfile>
    {
        Task CreateCoachProfileAsync(CoachProfile profile);
        Task<CoachProfile?> GetProfileBySlugAsync(string slug);
        Task<CoachProfile?> GetProfileByIdAsync(Guid id);
        Task<(IReadOnlyList<CoachProfile> Items, int TotalCount)> GetPagedCoachProfilesAsync(
            string? search,
            Guid? skillId,
            Guid? companyId,
            Guid? industryId,
            int page,
            int pageSize,
            List<Guid>? skillIds = null,
            List<string>? levels = null,
            int? minExperienceYears = null,
            int? maxExperienceYears = null,
            int? minPrice = null,
            int? maxPrice = null);
        Task UpdateCoachProfileAsync(CoachProfile updatedProfile);
        Task ReplaceWorkExperiencesAsync(Guid coachId, IEnumerable<CoachWorkExperience> workExperiences);
        void DeleteCoachProfile(Guid id);
        Task<int> GetTotalCoachCountAsync();
    }
}

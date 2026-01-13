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
        Task<(IReadOnlyList<CoachProfile> Items, int TotalCount)> GetPagedCoachProfilesAsync(string? search, Guid? skillId, Guid? companyId, int page, int pageSize);
        Task UpdateCoachProfileAsync(CoachProfile updatedProfile);
        void DeleteCoachProfile(Guid id);
        Task<int> GetTotalCoachCountAsync();
    }
}

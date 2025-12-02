using Intervu.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Domain.Repositories
{
    public interface IInterviewerProfileRepository : IRepositoryBase<InterviewerProfile>
    {
        Task<InterviewerProfile> GetProfileAsync();
        Task<InterviewerProfile?> GetProfileByIdAsync(int id);
        Task CreateInterviewerProfile(InterviewerProfile profile);
        Task UpdateInterviewerProfileAsync(InterviewerProfile updatedProfile);
        void DeleteInterviewerProfile(int id);
        Task<(IReadOnlyList<InterviewerProfile> Items, int TotalCount)> GetPagedInterviewerProfilesAsync(string? search, int? skillId, int? companyId, int page, int pageSize);

    }
}

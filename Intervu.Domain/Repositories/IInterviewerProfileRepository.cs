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
        //Task<InterviewerProfile> GetProfileAsync();
        Task<InterviewerProfile?> GetProfileBySlugAsync(string slug);
        Task<InterviewerProfile?> GetProfileByIdAsync(Guid id);
        Task CreateInterviewerProfileAsync(InterviewerProfile profile);
        Task UpdateInterviewerProfileAsync(InterviewerProfile updatedProfile);
        void DeleteInterviewerProfile(Guid id);
        Task<(IReadOnlyList<InterviewerProfile> Items, int TotalCount)> GetPagedInterviewerProfilesAsync(string? search, Guid? skillId, Guid? companyId, int page, int pageSize);
        Task<int> GetTotalInterviewersCountAsync();
    }
}

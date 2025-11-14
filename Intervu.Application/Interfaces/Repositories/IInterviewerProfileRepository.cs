using Intervu.Application.Common;
using Intervu.Application.DTOs.Interviewer;
using Intervu.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.Repositories
{
    public interface IInterviewerProfileRepository : IRepositoryBase<InterviewerProfile>
    {
        Task<InterviewerProfile> GetProfileAsync();
        Task<InterviewerProfile?> GetProfileByIdAsync(int id);
        Task CreateInterviewerProfile(InterviewerProfile interviewerProfile);
        Task UpdateInterviewerProfileAsync(InterviewerUpdateDto updatedProfile);
        void DeleteInterviewerProfile(int id);
        Task<PagedResult<InterviewerProfile>> GetPagedInterviewerProfilesAsync(int pageNumber, int pageSize);

    }
}

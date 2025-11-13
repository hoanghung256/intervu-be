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
        Task<InterviewerProfileDto> GetProfileByIdAsync(int id);
        Task CreateInterviewerProfile(InterviewerProfile interviewerProfile);
        Task UpdateInterviewerProfileAsync(InterviewerUpdateDto updatedProfile);
        void DeleteInterviewerProfile(int id);

    }
}

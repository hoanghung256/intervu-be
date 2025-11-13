using Intervu.Application.DTOs.Interviewer;
using Intervu.Domain.Entities.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.InterviewerProfile
{
    public interface IUpdateInterviewProfile
    {
        Task<InterviewerProfileDto> UpdateInterviewProfile(int id, InterviewerUpdateDto interviewerUpdateDto);
        Task<InterviewerViewDto> UpdateInterviewStatus(int id,InterviewerProfileStatus status);
    }
}

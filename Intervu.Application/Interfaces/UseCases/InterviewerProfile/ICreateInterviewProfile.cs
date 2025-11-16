using Intervu.Application.DTOs.Interviewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Interviewer
{
    public interface ICreateInterviewProfile
    {
        Task<InterviewerProfileDto> CreateInterviewRequest(InterviewerCreateDto interviewerCreateDto);
    }
}

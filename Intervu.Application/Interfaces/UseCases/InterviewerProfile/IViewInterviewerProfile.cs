using Intervu.Application.DTOs.Interviewer;
using Intervu.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.InterviewerProfile
{
    public interface IViewInterviewerProfile
    {
        Task<InterviewerProfileDto?> ViewOwnProfileAsync(Guid id);

        Task<InterviewerViewDto?> ViewProfileForIntervieweeAsync(string slug);
    }
}

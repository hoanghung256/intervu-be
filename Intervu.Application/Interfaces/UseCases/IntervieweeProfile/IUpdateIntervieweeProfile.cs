using Intervu.Application.DTOs.Interviewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.IntervieweeProfile
{
    public interface IUpdateIntervieweeProfile
    {
        Task<Domain.Entities.IntervieweeProfile> UpdateIntervieweeProfile(Guid id, string cvUrl);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.Interviewer;

namespace Intervu.Application.Interfaces.UseCases.InterviewerProfile
{
    public interface IGetAllInterviewers
    {
        Task<PagedResult<InterviewerProfileDto>> ExecuteAsync(GetInterviewerFilterRequest request);
    }
}

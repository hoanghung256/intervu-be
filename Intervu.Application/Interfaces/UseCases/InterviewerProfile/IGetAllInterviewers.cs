using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Application.Common;
using Intervu.Application.DTOs.Interviewer;

namespace Intervu.Application.Interfaces.UseCases.InterviewerProfile
{
    public interface IGetAllInterviewers
    {
        Task<PagedResult<InterviewerProfileDto>> ExecuteAsync(int page, int pageSize);
    }
}

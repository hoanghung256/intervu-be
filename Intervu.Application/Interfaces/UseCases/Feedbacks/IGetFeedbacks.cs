using Intervu.Domain.Entities;
using Intervu.Application.DTOs.Interviewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Application.DTOs.Feedback;
using Intervu.Application.DTOs.Common;

namespace Intervu.Application.Interfaces.UseCases.Feedbacks
{
    public interface IGetFeedbacks
    {
        Task<PagedResult<Feedback>> ExecuteAsync(GetFeedbackRequest request);
        Task<Feedback?> ExecuteAsync(Guid id);
    }
}

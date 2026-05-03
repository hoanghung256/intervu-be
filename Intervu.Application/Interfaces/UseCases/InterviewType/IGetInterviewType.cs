using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.InterviewType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.InterviewType
{
    public interface IGetInterviewType
    {
        Task<InterviewTypeDto> ExecuteAsync(Guid typeId);
        /// <param name="includeAllStatuses">When true (admin), returns all statuses. When false, only Active (default for public API).</param>
        Task<PagedResult<InterviewTypeDto>> ExecuteAsync(int pageSize, int currentPage, bool includeAllStatuses = false);
    }
}

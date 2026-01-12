using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.Coach;

namespace Intervu.Application.Interfaces.UseCases.CoachProfile
{
    public interface IGetAllCoach
    {
        Task<PagedResult<CoachProfileDto>> ExecuteAsync(GetCoachFilterRequest request);
    }
}

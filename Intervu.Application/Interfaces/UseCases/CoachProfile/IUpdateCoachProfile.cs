using Intervu.Application.DTOs.Coach;
using Intervu.Domain.Entities.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.CoachProfile
{
    public interface IUpdateCoachProfile
    {
        Task<CoachProfileDto> ExecuteAsync(Guid id, CoachUpdateDto coachUpdateDto);
        Task<CoachProfileDto> UpdateCoachStatus(Guid id, CoachProfileStatus status);
    }
}

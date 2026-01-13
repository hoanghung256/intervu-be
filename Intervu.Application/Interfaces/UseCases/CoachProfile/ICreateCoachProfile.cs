using Intervu.Application.DTOs.Coach;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.CoachProfile
{
    public interface ICreateCoachProfile
    {
        Task<CoachProfileDto> CreateCoachRequest(CoachCreateDto coachCreateDto);
    }
}

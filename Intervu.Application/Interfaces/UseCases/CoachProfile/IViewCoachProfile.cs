using Intervu.Application.DTOs.Coach;
using Intervu.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.CoachProfile
{
    public interface IViewCoachProfile
    {
        Task<CoachProfileDto?> ViewOwnProfileAsync(Guid id);

        Task<CoachViewDto?> ViewProfileForCandidateAsync(string slug);
    }
}

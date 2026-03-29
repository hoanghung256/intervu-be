using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Intervu.Domain.Entities;

namespace Intervu.Application.Interfaces.Repositories
{
    public interface IUserAssessmentRepository
    {
        Task AddAnswersAsync(IEnumerable<UserAssessmentAnswer> answers);
        Task<IReadOnlyList<UserAssessmentAnswer>> GetAnswersByAssessmentIdAsync(Guid assessmentId);
        Task<IReadOnlyList<Skill>> GetAllSkillsAsync();
    }
}

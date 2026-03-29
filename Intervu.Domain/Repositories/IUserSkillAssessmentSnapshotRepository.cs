using System;
using System.Threading.Tasks;
using Intervu.Domain.Entities;

namespace Intervu.Domain.Repositories
{
    public interface IUserSkillAssessmentSnapshotRepository
    {
        Task<UserSkillAssessmentSnapshot?> GetByUserIdAsync(Guid userId);
        Task UpsertSnapshotAsync(UserSkillAssessmentSnapshot snapshot);
    }
}

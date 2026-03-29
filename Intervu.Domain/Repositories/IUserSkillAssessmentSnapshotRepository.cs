using System;
using System.Threading.Tasks;
using Intervu.Domain.Entities;

namespace Intervu.Domain.Repositories
{
    public interface IUserSkillAssessmentSnapshotRepository
    {
        Task<UserSkillAssessmentSnapshot?> GetUserSkillAssessmentById(Guid userId);
        Task UpsertSnapshotAsync(UserSkillAssessmentSnapshot snapshot);
    }
}

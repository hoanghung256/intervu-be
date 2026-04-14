using System;
using System.Threading;
using System.Threading.Tasks;
using Intervu.Domain.Entities;

namespace Intervu.Domain.Repositories
{
    public interface IUserSkillAssessmentSnapshotRepository
    {
        Task<UserSkillAssessmentSnapshot?> GetUserSkillAssessmentById(Guid userId, CancellationToken cancellationToken = default);
        Task UpsertSnapshotAsync(UserSkillAssessmentSnapshot snapshot, CancellationToken cancellationToken = default);
        Task SaveAnswerJsonAsync(Guid userId, string answerJson, CancellationToken cancellationToken = default);
    }
}

using Intervu.Domain.Entities;

namespace Intervu.Domain.Repositories
{
    public interface ICoachInterviewServiceRepository : IRepositoryBase<CoachInterviewService>
    {
        Task<IEnumerable<CoachInterviewService>> GetByCoachIdAsync(Guid coachId);
        Task<CoachInterviewService?> GetByCoachAndTypeAsync(Guid coachId, Guid interviewTypeId);
        Task<CoachInterviewService?> GetByIdWithDetailsAsync(Guid id);
        Task<IEnumerable<CoachInterviewService>> GetByIdsAsync(IEnumerable<Guid> ids);
    }
}

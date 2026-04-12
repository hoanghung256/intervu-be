using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Domain.Repositories
{
    public interface IInterviewReportRepository : IRepositoryBase<InterviewReport>
    {
        Task<(List<InterviewReport> Items, int TotalCount)> GetPagedAsync(
            int page,
            int pageSize,
            InterviewReportStatus? status = null,
            string? search = null,
            Guid? reporterId = null);

        Task<bool> ExistsByRoomIdAsync(Guid interviewRoomId);
        Task<InterviewReport?> GetByRoomIdAsync(Guid interviewRoomId);
        Task<int> GetPendingCountAsync();
    }
}

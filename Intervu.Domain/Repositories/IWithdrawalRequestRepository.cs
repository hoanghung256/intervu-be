using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Domain.Repositories
{
    public interface IWithdrawalRequestRepository : IRepositoryBase<WithdrawalRequest>
    {
        Task<WithdrawalRequest?> GetByIdWithUserAsync(Guid id);
        Task<(IReadOnlyList<WithdrawalRequest> Items, int TotalCount)> GetPagedAsync(
            WithdrawalStatus? status, int page, int pageSize);
        Task<(IReadOnlyList<WithdrawalRequest> Items, int TotalCount)> GetPagedByUserAsync(
            Guid userId, int page, int pageSize);
        Task<List<WithdrawalRequest>> GetByUserIdAsync(Guid userId);
        Task<bool> HasPendingWithdrawalAsync(Guid userId);
    }
}

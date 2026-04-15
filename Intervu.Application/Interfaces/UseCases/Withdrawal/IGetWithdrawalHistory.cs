using Intervu.Application.DTOs.Withdrawal;

namespace Intervu.Application.Interfaces.UseCases.Withdrawal
{
    public interface IGetWithdrawalHistory
    {
        Task<(IReadOnlyList<WithdrawalResponseDto> Items, int TotalCount)> ExecuteAsync(Guid userId, int page, int pageSize);
    }
}

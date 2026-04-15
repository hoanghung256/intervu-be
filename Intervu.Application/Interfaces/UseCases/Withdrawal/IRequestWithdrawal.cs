using Intervu.Application.DTOs.Withdrawal;

namespace Intervu.Application.Interfaces.UseCases.Withdrawal
{
    public interface IRequestWithdrawal
    {
        Task<WithdrawalResponseDto> ExecuteAsync(Guid userId, RequestWithdrawalDto request);
    }
}

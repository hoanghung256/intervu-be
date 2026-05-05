using Intervu.Application.DTOs.Payment;

namespace Intervu.Application.Interfaces.UseCases.Admin
{
    public interface IGetPayOsPayoutAccountBalance
    {
        Task<PayoutAccountBalanceDto> ExecuteAsync(CancellationToken cancellationToken = default);
    }
}

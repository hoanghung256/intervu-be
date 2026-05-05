using Intervu.Application.DTOs.Payment;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.Admin;

namespace Intervu.Application.UseCases.Admin
{
    public class GetPayOsPayoutAccountBalance(IPaymentService paymentService) : IGetPayOsPayoutAccountBalance
    {
        private readonly IPaymentService _paymentService = paymentService;

        public Task<PayoutAccountBalanceDto> ExecuteAsync(CancellationToken cancellationToken = default) =>
            _paymentService.GetPayoutAccountBalanceAsync(cancellationToken);
    }
}

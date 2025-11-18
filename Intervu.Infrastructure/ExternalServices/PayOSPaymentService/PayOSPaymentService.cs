using Intervu.Application.Interfaces.ExternalServices;
using PayOS.Models.V1.Payouts;
using PayOS.Models.V2.PaymentRequests;

namespace Intervu.Infrastructure.ExternalServices.PayOSPaymentService
{
    public class PayOSPaymentService : IPaymentService
    {
        private readonly PaymentClient _paymentClient;
        private readonly PayoutClient _payoutClient;
        private readonly string _returnUrl;
        private readonly string _cancelUrl;

        public PayOSPaymentService(PaymentClient paymentClient, PayoutClient payoutClient, string returnUrl, string cancelUrl) 
        {
            _paymentClient = paymentClient;
            _payoutClient = payoutClient;
            _returnUrl = returnUrl;
            _cancelUrl = cancelUrl;
        }

        public async Task<string> CreatePaymentOrderAsync(int? orderCode, int ammount, string description)
        {
            var paymentRequest = new CreatePaymentLinkRequest
            {
                OrderCode = orderCode ?? CreateOrderCode(),
                Amount = ammount,
                Description = description,
                ReturnUrl = _returnUrl,
                CancelUrl = _cancelUrl,
                ExpiredAt = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds()
            };

            CreatePaymentLinkResponse paymentLink = await _paymentClient.Client.PaymentRequests.CreateAsync(paymentRequest);

            return paymentLink.CheckoutUrl;
        }

        public async Task<bool> CreateSpendOrderAsync(int amount, string description, string targetBankId, string targetBankAccountNumber)
        {
            var payoutRequest = new PayoutRequest
            {
                Amount = amount,
                Description = description,
                ToAccountNumber = targetBankAccountNumber,
                ToBin = targetBankId
            };
            await _payoutClient.Client.Payouts.CreateAsync(payoutRequest);

            return true;
        }

        //public Task<bool> VerifyPaymentAsync(PayOSWebhookPayload payload)
        //{
        //    int orderCode = payload.Data!.OrderCode;
        //    string status = payload.Data!.Status!;

        //    Console.WriteLine($"order Code {orderCode} {status}");

        //    bool isSuccess = status.Equals("paid", StringComparison.OrdinalIgnoreCase);

        //    return Task.FromResult(isSuccess);
        //}

        public async Task RegisterWebhooks()
        {
            await _paymentClient.Client.Webhooks.ConfirmAsync("https://pn3tc7bj-7118.asse.devtunnels.ms/weatherforecast/payos-webhook-test");
        }

        private int CreateOrderCode()
        {
            int timePart = (int)(DateTime.UtcNow.Ticks % 1_000_000_000); // Under 1 billion
            int randomPart = Random.Shared.Next(100, 999); // 3-digit

            return timePart ^ randomPart;
        }
    }
}

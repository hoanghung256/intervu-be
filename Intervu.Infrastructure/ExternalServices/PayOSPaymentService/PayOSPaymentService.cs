using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Infrastructure.ExternalServices.PayOSPaymentService.Interfaces;
using PayOS;
using PayOS.Models.V1.Payouts;
using PayOS.Models.V2.PaymentRequests;

namespace Intervu.Infrastructure.ExternalServices.PayOSPaymentService
{
    public class PayOSPaymentService : IPaymentService
    {
        private readonly PaymentClient _paymentClient;
        private readonly PayoutClient _payoutClient;

        public PayOSPaymentService(PaymentClient paymentClient, PayoutClient payoutClient) 
        {
            _paymentClient = paymentClient;
            _payoutClient = payoutClient;
        }

        public async Task<string> CreatePaymentOrderAsync(int? orderCode, int ammount, string description)
        {
            var paymentRequest = new CreatePaymentLinkRequest
            {
                OrderCode = orderCode ?? CreateOrderCode(),
                Amount = ammount,
                Description = description,
                ReturnUrl = "https://your-url.com",
                CancelUrl = "https://your-url.com",
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

        public Task<bool> VerifyPaymentAsync(string paymentId, string orderId, string signature)
        {
            throw new NotImplementedException();
        }

        private int CreateOrderCode()
        {
            int timePart = (int)(DateTime.UtcNow.Ticks % 1_000_000_000); // Under 1 billion
            int randomPart = Random.Shared.Next(100, 999); // 3-digit

            return timePart ^ randomPart;
        }
    }
}

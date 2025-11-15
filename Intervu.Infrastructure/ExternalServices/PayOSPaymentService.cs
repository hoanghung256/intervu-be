using Intervu.Application.Interfaces.ExternalServices;
using PayOS;
using PayOS.Models.V2.PaymentRequests;

namespace Intervu.Infrastructure.ExternalServices
{
    public class PayOSPaymentService : IPaymentService
    {
        private readonly PayOSClient _payos;

        public PayOSPaymentService(PayOSClient payos) 
        {
            _payos = payos;
        }

        public async Task<string> CreatePaymentOrderAsync(int ammount, string description)
        {
            var paymentRequest = new CreatePaymentLinkRequest
            {
                OrderCode = CreateOrderCode(),
                Amount = ammount,
                Description = description,
                ReturnUrl = "https://your-url.com",
                CancelUrl = "https://your-url.com"
            };

            CreatePaymentLinkResponse paymentLink = await _payos.PaymentRequests.CreateAsync(paymentRequest);

            return paymentLink.CheckoutUrl;
        }

        public Task<bool> CreateSpendOrderAsync(int amount, string description, string targetBankId, string targetBankAccountNumber)
        {
            throw new NotImplementedException();
        }

        public Task<bool> VerifyPaymentAsync(string orderId)
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

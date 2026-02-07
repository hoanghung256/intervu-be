using Intervu.Application.Interfaces.ExternalServices;
using PayOS.Models.V1.Payouts;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;

namespace Intervu.Infrastructure.ExternalServices.PayOSPaymentService
{
    public class PayOSPaymentService : IPaymentService
    {
        public readonly PaymentClient _paymentClient;
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

        public async Task<string> CreatePaymentOrderAsync(int orderCode, int ammount, string description, string returnUrl)
        {
            var paymentRequest = new CreatePaymentLinkRequest
            {
                OrderCode = orderCode,
                Amount = ammount,
                Description = description,
                ReturnUrl = returnUrl,
                CancelUrl = returnUrl,
                ExpiredAt = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds()
            };

            CreatePaymentLinkResponse paymentLink = await _paymentClient.Client.PaymentRequests.CreateAsync(paymentRequest);

            return paymentLink.CheckoutUrl;
        }

        public async Task<bool> CreateSpendOrderAsync(int amount, string description, string targetBankId, string targetBankAccountNumber)
        {
            // Guard: Do not call external API with non-positive amounts
            if (amount <= 0)
            {
                return false;
            }

            var payoutRequest = new PayoutRequest
            {
                Amount = amount,
                Description = description,
                ToAccountNumber = targetBankAccountNumber,
                ToBin = targetBankId
            };

            try
            {
                await _payoutClient.Client.Payouts.CreateAsync(payoutRequest);
                return true;
            }
            catch (Exception)
            {
                // Swallow external API exception here so background services don't crash the host.
                // Higher-level code can react to a `false` return value if needed.
                return false;
            }
        }

        public (bool isValid, int orderCode) VerifyPayment(object payload)
        {
            if (payload is not Webhook payloadCasting) return (false, 0);

            //WebhookData webhookData = await _paymentClient.Client.Webhooks.VerifyAsync(payloadCasting);

            string? calculated = _paymentClient.Client.Crypto.CreateSignatureFromObject(payloadCasting.Data, _paymentClient.Client.ChecksumKey);

            if (!string.IsNullOrEmpty(calculated) && !string.IsNullOrEmpty(payloadCasting.Signature))
            {
                return (true, (int) payloadCasting.Data.OrderCode);
            }

            return (false, 0);
        }

        public async Task RegisterWebhooks()
        {
            await _paymentClient.Client.Webhooks.ConfirmAsync("https://pn3tc7bj-7118.asse.devtunnels.ms/api/v1/interview-booking/webhook");
        }
    }
}

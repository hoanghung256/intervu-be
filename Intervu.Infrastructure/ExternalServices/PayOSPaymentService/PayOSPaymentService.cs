using Intervu.Application.Interfaces.ExternalServices;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<PayOSPaymentService> _logger;

        public PayOSPaymentService(
            PaymentClient paymentClient,
            PayoutClient payoutClient,
            string returnUrl,
            string cancelUrl,
            ILogger<PayOSPaymentService> logger)
        {
            _paymentClient = paymentClient;
            _payoutClient = payoutClient;
            _returnUrl = returnUrl;
            _cancelUrl = cancelUrl;
            _logger = logger;
        }

        public async Task<string> CreatePaymentOrderAsync(int orderCode, int ammount, string description, string returnUrl, long expiredAfter = 4)
        {
            var paymentRequest = new CreatePaymentLinkRequest
            {
                OrderCode = orderCode,
                Amount = ammount,
                Description = description,
                ReturnUrl = returnUrl,
                CancelUrl = returnUrl,
                ExpiredAt = DateTimeOffset.UtcNow.AddMinutes(expiredAfter).ToUnixTimeSeconds()
            };

            CreatePaymentLinkResponse paymentLink = await _paymentClient.Client.PaymentRequests.CreateAsync(paymentRequest);

            return paymentLink.CheckoutUrl;
        }

        public async Task<bool> CreateSpendOrderAsync(int amount, string description, string targetBankId, string targetBankAccountNumber)
        {
            // Guard: Do not call external API with non-positive amounts
            if (amount <= 0)
            {
                _logger.LogWarning("Skip payout because amount is invalid. Amount: {Amount}", amount);
                return false;
            }

            var maskedAccount = string.IsNullOrWhiteSpace(targetBankAccountNumber)
                ? string.Empty
                : (targetBankAccountNumber.Length <= 4
                    ? "****"
                    : $"****{targetBankAccountNumber[^4..]}");

            _logger.LogInformation(
                "Creating payout order. Amount: {Amount}, Description: {Description}, TargetBankId: {TargetBankId}, TargetAccount: {MaskedAccount}",
                amount,
                description,
                targetBankId,
                maskedAccount);

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
                _logger.LogInformation("Payout order created successfully.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create payout order. Amount: {Amount}, TargetBankId: {TargetBankId}, TargetAccount: {MaskedAccount}", amount, targetBankId, maskedAccount);
                throw new Exception("PAYMENT Failed", ex);
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

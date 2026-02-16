namespace Intervu.Application.Interfaces.ExternalServices
{
    public interface IPaymentService
    {
        /// <summary>
        /// Creates a payment order and returns the checkout URL.
        /// </summary>
        /// <param name="orderCode">
        /// This will created by DB with IDENTITY(1,1)
        /// </param>
        /// <param name="ammount">Total amount of the payment.</param>
        /// <param name="description">Description shown on the payment page.</param>
        /// <returns>The checkout URL for the created payment order.</returns>
        Task<string> CreatePaymentOrderAsync(int orderCode, int ammount, string description, string returnUrl, long expiredAfter = 4);

        (bool isValid, int orderCode) VerifyPayment(object payload);

        Task<bool> CreateSpendOrderAsync(int amount, string description, string targetBankId, string targetBankAccountNumber);

        Task RegisterWebhooks();
    }
}

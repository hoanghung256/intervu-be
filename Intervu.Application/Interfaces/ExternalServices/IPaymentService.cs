namespace Intervu.Application.Interfaces.ExternalServices
{
    public interface IPaymentService
    {
        /// <summary>
        /// Creates a payment order and returns the checkout URL.
        /// </summary>
        /// <param name="orderCode">
        /// Optional custom order code.  
        /// If null, the system will automatically generate a new unique order code.
        /// Use when you want to map a specific internal order to a PayOS transaction.
        /// </param>
        /// <param name="ammount">Total amount of the payment.</param>
        /// <param name="description">Description shown on the payment page.</param>
        /// <returns>The checkout URL for the created payment order.</returns>
        Task<string> CreatePaymentOrderAsync(int? orderCode, int ammount, string description);

        bool VerifyPaymentAsync(object payload);

        Task<bool> CreateSpendOrderAsync(int amount, string description, string targetBankId, string targetBankAccountNumber);

        Task RegisterWebhooks();
    }
}

namespace Intervu.Application.Interfaces.ExternalServices
{
    public interface IPaymentService
    {
        Task<string> CreatePaymentOrderAsync(int? orderCode, int ammount, string description);

        //Task<bool> VerifyPaymentAsync(Webhook payload);

        Task<bool> CreateSpendOrderAsync(int amount, string description, string targetBankId, string targetBankAccountNumber);

        Task RegisterWebhooks();
    }
}

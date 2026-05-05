namespace Intervu.Application.DTOs.Payment
{
    /// <summary>PayOS payout (spend) account snapshot from GET /v1/payouts-account/balance.</summary>
    public sealed class PayoutAccountBalanceDto
    {
        public string AccountNumber { get; set; } = "";
        public string AccountName { get; set; } = "";
        public string Currency { get; set; } = "";
        /// <summary>Balance as returned by PayOS (string to preserve exact value).</summary>
        public string Balance { get; set; } = "";
    }
}

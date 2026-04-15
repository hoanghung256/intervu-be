namespace Intervu.Application.DTOs.Withdrawal
{
    public class RequestWithdrawalDto
    {
        [Range(2000, int.MaxValue)]
        public int Amount { get; set; }
        public string? Notes { get; set; }
    }
}

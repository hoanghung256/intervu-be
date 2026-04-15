using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.DTOs.Withdrawal
{
    public class WithdrawalResponseDto
    {
        public Guid Id { get; set; }
        public int Amount { get; set; }
        public WithdrawalStatus Status { get; set; }
        public string BankBinNumber { get; set; } = string.Empty;
        public string BankAccountNumber { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}

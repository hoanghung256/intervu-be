using Intervu.Domain.Abstractions.Entity;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Domain.Entities
{
    public class WithdrawalRequest : EntityAuditable<Guid>
    {
        public Guid UserId { get; set; }
        public int Amount { get; set; }
        public WithdrawalStatus Status { get; set; }
        public string BankBinNumber { get; set; } = string.Empty;
        public string BankAccountNumber { get; set; } = string.Empty;
        public string BankAccountNumberMasked { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime? ProcessedAt { get; set; }

        // Navigation
        public User? User { get; set; }
    }
}

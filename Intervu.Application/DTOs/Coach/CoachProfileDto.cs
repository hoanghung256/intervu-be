using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.DTOs.Coach
{
    public class CoachProfileDto : CoachViewDto
    {
        public int? CurrentAmount { get; set; }

        public CoachProfileStatus Status { get; set; }

        public string? BankBinNumber { get; set; }

        public string? BankAccountNumber { get; set; }
    }
}

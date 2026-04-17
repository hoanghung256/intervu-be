using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.DTOs.User
{
    public class CurrentUserProfileDto
    {
        public UserDto User { get; set; } = null!;
        public UserRole Role { get; set; }
        public CurrentUserCandidateProfileDto? CandidateProfile { get; set; }
        public CurrentUserCoachProfileDto? CoachProfile { get; set; }
    }

    public class CurrentUserCandidateProfileDto
    {
        public Guid Id { get; set; }
        public string? BankBinNumber { get; set; }
        public string? BankAccountNumber { get; set; }
    }

    public class CurrentUserCoachProfileDto
    {
        public Guid Id { get; set; }
        public string? BankBinNumber { get; set; }
        public string? BankAccountNumber { get; set; }
    }
}
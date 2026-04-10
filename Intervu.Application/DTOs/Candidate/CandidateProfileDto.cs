using Intervu.Application.DTOs.Industry;
using Intervu.Application.DTOs.Skill;
using Intervu.Application.DTOs.User;

namespace Intervu.Application.DTOs.Candidate
{
    public class CandidateProfileDto
    {
        public Guid Id { get; set; }
        public UserDto User { get; set; } = null!;
        public string? CVUrl { get; set; }
        public string? PortfolioUrl { get; set; }
        public ICollection<SkillDto> Skills { get; set; } = new List<SkillDto>();
        public ICollection<IndustryDto> Industries { get; set; } = new List<IndustryDto>();
        public ICollection<CandidateCertificateDto> Certificates { get; set; } = new List<CandidateCertificateDto>();
        public ICollection<CandidateWorkExperienceDto> WorkExperiences { get; set; } = new List<CandidateWorkExperienceDto>();
        public string? Bio { get; set; }
        public int CurrentAmount { get; set; }
        public string? BankBinNumber { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? AIEvaluation { get; set; }
    }
}

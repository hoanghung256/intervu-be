using AutoMapper.Configuration.Annotations;
using Intervu.Application.DTOs.Company;
using Intervu.Application.DTOs.Industry;
using Intervu.Application.DTOs.Skill;
using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.DTOs.Coach
{
    public class CoachViewDto
    {
        public Guid Id { get; set; }
        [Ignore]
        public UserDto? User { get; set; }

        public string? PortfolioUrl { get; set; }

        public int? ExperienceYears { get; set; }

        public string? Bio { get; set; }

        public string? CurrentJobTitle { get; set; }

        public ICollection<CompanyDto> Companies { get; set; } = new List<CompanyDto>();

        public ICollection<SkillDto> Skills { get; set; } = new List<SkillDto>();

        public ICollection<IndustryDto> Industries { get; set; } = new List<IndustryDto>();

        public List<string>? CertificationLinks { get; set; }

        public ICollection<CoachWorkExperienceDto> WorkExperiences { get; set; } = new List<CoachWorkExperienceDto>();
    }
}

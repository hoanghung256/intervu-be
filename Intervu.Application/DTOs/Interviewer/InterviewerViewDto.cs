using AutoMapper.Configuration.Annotations;
using Intervu.Application.DTOs.Company;
using Intervu.Application.DTOs.Skill;
using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.DTOs.Interviewer
{
    public class InterviewerViewDto
    {
        public int Id { get; set; }
        [Ignore]
        public UserDto User { get; set; }

        public string? PortfolioUrl { get; set; }

        public int? ExperienceYears { get; set; }

        public string? Bio { get; set; }

        public ICollection<CompanyDto> Companies { get; set; } = new List<CompanyDto>();

        public ICollection<SkillDto> Skills { get; set; } = new List<SkillDto>();
    }
}

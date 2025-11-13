using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.DTOs.Interviewer
{
    public class InterviewerViewDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? ProfilePicture { get; set; }
        public string? PortfolioUrl { get; set; }
        public string Specializations { get; set; }
        public string ProgrammingLanguages { get; set; }
        public string Company { get; set; }
        public int ExperienceYears { get; set; }
        public string Bio { get; set; }
    }
}

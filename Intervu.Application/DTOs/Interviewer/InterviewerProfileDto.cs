using Intervu.Application.DTOs.Company;
using Intervu.Application.DTOs.Skill;
using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.DTOs.Interviewer
{
    public class InterviewerProfileDto : InterviewerViewDto
    {
        public int? CurrentAmount { get; set; }

        public InterviewerProfileStatus Status { get; set; }
    }
}

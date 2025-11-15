using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.DTOs.Interviewer
{
    public class GetInterviewerFilterRequest
    {
        public string? Search { get; set; }
        public int? CompanyId { get; set; }
        public int? SkillId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 24;
    }
}

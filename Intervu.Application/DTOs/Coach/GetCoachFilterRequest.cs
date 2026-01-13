using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.DTOs.Coach
{
    public class GetCoachFilterRequest
    {
        public string? Search { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? SkillId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 24;
    }
}

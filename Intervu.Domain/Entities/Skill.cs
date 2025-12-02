using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Domain.Abstractions.Entity;

namespace Intervu.Domain.Entities
{
    public class Skill : EntityBase<int>
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Navigation
        public ICollection<InterviewerProfile> InterviewerProfiles { get; set; } = new List<InterviewerProfile>();
    }

}

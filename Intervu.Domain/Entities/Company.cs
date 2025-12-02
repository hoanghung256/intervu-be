using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Domain.Abstractions.Entity;

namespace Intervu.Domain.Entities
{
    public class Company : EntityBase<int>
    {
        public string Name { get; set; }
        public string Website { get; set; }
        public string LogoPath { get; set; }
        public ICollection<InterviewerProfile> InterviewerProfiles { get; set; } = new List<InterviewerProfile>(); 
    }
}

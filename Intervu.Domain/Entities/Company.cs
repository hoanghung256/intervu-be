using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Domain.Abstractions.Entity;

namespace Intervu.Domain.Entities
{
    public class Company : EntityBase<Guid>
    {
        public required string Name { get; set; }
        public required string Website { get; set; }
        public required string LogoPath { get; set; }
        public ICollection<CoachProfile> CoachProfiles { get; set; } = new List<CoachProfile>();
    }
}

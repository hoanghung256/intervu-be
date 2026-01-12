using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Domain.Abstractions.Entity;

namespace Intervu.Domain.Entities
{
    public class Skill : EntityBase<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Navigation
        public ICollection<CoachProfile> CoachProfiles { get; set; } = new List<CoachProfile>();
    }

}

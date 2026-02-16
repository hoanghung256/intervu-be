using Intervu.Domain.Abstractions.Entity;
using Intervu.Domain.Entities.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Domain.Entities
{
    public class InterviewType : EntityBase<Guid>
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsCoding { get; set; }

        [Range(30, 180)]
        public int DurationMinutes { get; set; }

        [Range(0, int.MaxValue)]
        public int BasePrice { get; set; }

        public InterviewTypeStatus Status { get; set; }
    }
}

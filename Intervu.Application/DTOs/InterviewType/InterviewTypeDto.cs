using Intervu.Domain.Entities.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.DTOs.InterviewType
{
    public class InterviewTypeDto
    {
        [MaxLength(150)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        public bool IsCoding { get; set; }

        [Range(1, 180)]
        public int DurationMinutes { get; set; }

        [Range(0, int.MaxValue)]
        public int BasePrice { get; set; }

        public InterviewTypeStatus Status { get; set; }
    }
}

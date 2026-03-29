using Intervu.Domain.Entities;
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
        public Guid Id { get; set; }
        [MaxLength(150)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        public bool IsCoding { get; set; }

        [Range(0, int.MaxValue)]
        public int MinPrice { get; set; }

        [Range(0, int.MaxValue)]
        public int MaxPrice { get; set; }

        [Range(15, 300)]
        public int SuggestedDurationMinutes { get; set; }

        public List<EvaluationItem>? EvaluationStructure { get; set; }

        public InterviewTypeStatus Status { get; set; }
    }
}

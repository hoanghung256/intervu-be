using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.DTOs.RescheduleRequest
{
    public class RespondToRescheduleRequestDto
    {
        [Required]
        public bool IsApproved { get; set; }

        public string? RejectionReason { get; set; }
    }
}

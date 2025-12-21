using System;
using System.Collections.Generic;

namespace Intervu.Application.DTOs.Admin
{
    public class InterviewerAdminDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Specialization { get; set; }
        public int Experience { get; set; } // Changed from int? to int
        public string? Bio { get; set; }
        public decimal HourlyRate { get; set; } // Changed from decimal? to decimal
        public DateTime CreatedAt { get; set; }
    }
}

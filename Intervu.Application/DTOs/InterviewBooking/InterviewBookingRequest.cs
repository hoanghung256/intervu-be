using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.DTOs.InterviewBooking
{
    public class InterviewBookingRequest
    {
        public Guid CoachId { get; set; }
        public Guid CoachAvailabilityId { get; set; }
        public string ReturnUrl { get; set; } = string.Empty;
    }
}

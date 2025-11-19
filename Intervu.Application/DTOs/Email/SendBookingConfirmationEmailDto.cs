using System;
using System.Collections.Generic;

namespace Intervu.Application.DTOs.Email
{
    public class SendBookingConfirmationEmailDto
    {
        public string To { get; set; }
        public string CandidateName { get; set; }
        public string InterviewerName { get; set; }
        public DateTime InterviewDate { get; set; }
        public string InterviewTime { get; set; }
        public string Position { get; set; }
        public int Duration { get; set; }
        public string BookingID { get; set; }
        public string JoinLink { get; set; }
        public string RescheduleLink { get; set; }
    }
}

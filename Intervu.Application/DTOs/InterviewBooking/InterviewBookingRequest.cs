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
        public Guid CoachInterviewServiceId { get; set; }
        public DateTime StartTime { get; set; }
        public string ReturnUrl { get; set; } = string.Empty;

        /// <summary>
        /// Optional: roadmap node skill_id this booking targets — propagated to InterviewRoom
        /// so the post-interview update can deterministically update the originating node.
        /// </summary>
        public string? RoadmapNodeId { get; set; }
    }
}

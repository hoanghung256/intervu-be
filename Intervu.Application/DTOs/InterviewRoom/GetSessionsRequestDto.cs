using Intervu.Domain.Entities.Constants;
using System.Collections.Generic;

namespace Intervu.Application.DTOs.InterviewRoom
{
    public class GetSessionsRequestDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Filter sessions by the derived session status (can pass multiple).
        /// Example: Statuses=0&amp;Statuses=1 returns sessions whose active round is Scheduled or OnGoing.
        /// </summary>
        public List<InterviewRoomStatus>? Statuses { get; set; }
    }
}

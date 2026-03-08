using Intervu.Domain.Entities.Constants;
using System.Collections.Generic;

namespace Intervu.Application.DTOs.InterviewRoom
{
    public class GetInterviewRoomsRequestDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        
        /// <summary>
        /// Filter by statuses (optional): Can filter by multiple statuses
        /// Example: Statuses=0&Statuses=1 for Scheduled and OnGoing
        /// </summary>
        public List<InterviewRoomStatus>? Statuses { get; set; }
        
        /// <summary>
        /// Search by problem name or participant name (optional)
        /// </summary>
        public string? SearchQuery { get; set; }
    }
}

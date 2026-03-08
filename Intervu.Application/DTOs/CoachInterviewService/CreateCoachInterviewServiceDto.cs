using System.ComponentModel.DataAnnotations;

namespace Intervu.Application.DTOs.CoachInterviewService
{
    /// <summary>
    /// Request DTO for a coach to add an interview service they offer
    /// </summary>
    public class CreateCoachInterviewServiceDto
    {
        [Required]
        public Guid InterviewTypeId { get; set; }

        /// <summary>
        /// Coach's custom price — must be within InterviewType.MinPrice..MaxPrice
        /// </summary>
        [Required]
        [Range(0, int.MaxValue)]
        public int Price { get; set; }

        /// <summary>
        /// Coach's custom duration in minutes (15–300)
        /// </summary>
        [Required]
        [Range(15, 300)]
        public int DurationMinutes { get; set; }
    }
}

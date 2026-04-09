using System.ComponentModel.DataAnnotations;
using Intervu.Domain.Abstractions.Validation;

namespace Intervu.Application.DTOs.CoachInterviewService
{
    /// <summary>
    /// Request DTO to update a coach's interview service pricing/duration
    /// </summary>
    public class UpdateCoachInterviewServiceDto
    {
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
        [MultipleOf(30, ErrorMessage = "Duration must be a multiple of 30 minutes.")]
        public int DurationMinutes { get; set; }
    }
}

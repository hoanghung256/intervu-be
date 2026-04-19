using System.ComponentModel.DataAnnotations;

namespace Intervu.Application.DTOs.Feedback
{
    public class UpdateFeedbackDto
    {
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [MinLength(1)]
        [MaxLength(1000)]
        public string Comments { get; set; } = string.Empty;
    }
}

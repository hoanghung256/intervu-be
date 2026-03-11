using Intervu.Domain.Abstractions.Entity;
using Intervu.Domain.Entities.Constants;
using System.ComponentModel.DataAnnotations;

namespace Intervu.Domain.Entities
{
    public class InterviewType : EntityBase<Guid>
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public bool IsCoding { get; set; }

        [Range(0, int.MaxValue)]
        public int MinPrice { get; set; }

        [Range(0, int.MaxValue)]
        public int MaxPrice { get; set; }

        [Range(15, 300)]
        public int SuggestedDurationMinutes { get; set; }

        public InterviewTypeStatus Status { get; set; }
    }
}

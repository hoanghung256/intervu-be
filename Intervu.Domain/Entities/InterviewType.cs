using Intervu.Domain.Abstractions.Entity;
using Intervu.Domain.Abstractions.Validation;
using Intervu.Domain.Entities.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

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
        [MultipleOf(30, ErrorMessage = "Suggested duration must be a multiple of 30 minutes.")]
        public int SuggestedDurationMinutes { get; set; }

        // Store the evaluation structure as a JSON string in the database, EF purpose
        [Column(TypeName = "jsonb")]
        public string? EvaluationStructureJson { get; set; }

        // Use this field for application logic; it will be ignored by EF Core and not mapped to the database
        [NotMapped]
        public List<EvaluationItem>? EvaluationStructure
        {
            get => string.IsNullOrEmpty(EvaluationStructureJson)
                   ? null
                   : JsonSerializer.Deserialize<List<EvaluationItem>>(EvaluationStructureJson);
            set => EvaluationStructureJson = value == null
                   ? null
                   : JsonSerializer.Serialize(value);
        }

        public InterviewTypeStatus Status { get; set; }
    }
}

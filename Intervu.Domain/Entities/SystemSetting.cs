using Intervu.Domain.Abstractions.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Intervu.Domain.Entities
{
    public class SystemSetting : EntityBase<string>
    {
        [Required]
        public string Value { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Description { get; set; }
    }
}

using Intervu.Domain.Abstractions.Entity;

namespace Intervu.Domain.Entities
{
    public class Tag : EntityBase<Guid>
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public ICollection<QuestionTag> QuestionTags { get; set; } = new List<QuestionTag>();
    }
}

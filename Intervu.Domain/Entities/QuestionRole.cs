using Intervu.Domain.Entities.Constants.QuestionConstants;

namespace Intervu.Domain.Entities
{
    public class QuestionRole
    {
        public Guid QuestionId { get; set; }
        public Question Question { get; set; } = null!;

        public Role Role { get; set; }
    }
}

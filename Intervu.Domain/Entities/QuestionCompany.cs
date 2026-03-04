namespace Intervu.Domain.Entities
{
    public class QuestionCompany
    {
        public Guid QuestionId { get; set; }
        public Question Question { get; set; } = null!;

        public Guid CompanyId { get; set; }
        public Company Company { get; set; } = null!;
    }
}

using Intervu.Domain.Entities.Constants.QuestionConstants;
using System;

namespace Intervu.Domain.Entities
{
    public class QuestionReport
    {
        public Guid Id { get; set; }
        public Guid QuestionId { get; set; }
        public Question Question { get; set; } = null!;

        public Guid ReportedBy { get; set; }
        public User? Reporter { get; set; }

        public string Reason { get; set; } = null!;
        public QuestionReportStatus Status { get; set; } = QuestionReportStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
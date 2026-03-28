using Intervu.Domain.Abstractions.Entity;
using Intervu.Domain.Entities.Constants;
using System;

namespace Intervu.Domain.Entities
{
    public class GeneratedQuestion : EntityBase<Guid>
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public GeneratedQuestionStatus Status { get; set; } = GeneratedQuestionStatus.PendingReview;
        public Guid InterviewRoomId { get; set; }
        public virtual InterviewRoom InterviewRoom { get; set; } = null!;
    }
}

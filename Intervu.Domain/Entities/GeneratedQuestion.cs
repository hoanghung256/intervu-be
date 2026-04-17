using Intervu.Domain.Abstractions.Entity;
using Intervu.Domain.Entities.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Intervu.Domain.Entities
{
    public class GeneratedQuestion : EntityBase<Guid>
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public GeneratedQuestionStatus Status { get; set; } = GeneratedQuestionStatus.PendingReview;
        public Guid InterviewRoomId { get; set; }
        public virtual InterviewRoom InterviewRoom { get; set; } = null!;

        [Column(TypeName = "jsonb")]
        public string? TagIdsJson { get; set; }

        [NotMapped]
        public List<Guid> TagIds
        {
            get => string.IsNullOrEmpty(TagIdsJson)
                   ? new List<Guid>()
                   : JsonSerializer.Deserialize<List<Guid>>(TagIdsJson) ?? new List<Guid>();
            set => TagIdsJson = JsonSerializer.Serialize(value);
        }
    }
}

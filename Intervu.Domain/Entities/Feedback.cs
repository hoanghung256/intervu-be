using Intervu.Domain.Abstractions.Entity;
using System;

namespace Intervu.Domain.Entities
{
    public class Feedback : EntityBase<Guid>
    {
        /// <summary>
        /// EntityBase.Id represents FeedbackId
        /// </summary>
        public Guid InterviewerId { get; set; }

        public Guid StudentId { get; set; }

        public Guid InterviewRoomId { get; set; }

        public int Rating { get; set; }

        public string Comments { get; set; }

        /// <summary>
        /// AIAnalysis stored as JSON string
        /// </summary>
        public string AIAnalysis { get; set; }
    }
}

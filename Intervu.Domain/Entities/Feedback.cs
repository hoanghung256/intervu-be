using Intervu.Domain.Abstractions.Entity;
using System;

namespace Intervu.Domain.Entities
{
    public class Feedback : EntityBase<Guid>
    {
        /// <summary>
        /// EntityBase.Id represents FeedbackId
        /// </summary>
        public Guid CoachId { get; set; }

        public Guid CandidateId { get; set; }

        public Guid InterviewRoomId { get; set; }

        public int Rating { get; set; }

        public string Comments { get; set; }

        /// <summary>
        /// AIAnalysis stored as JSON string
        /// </summary>
        public string AIAnalysis { get; set; }
    }
}

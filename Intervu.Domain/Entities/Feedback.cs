using Intervu.Domain.Abstractions.Entities;
using System;

namespace Intervu.Domain.Entities
{
    public class Feedback : EntityBase<int>
    {
        /// <summary>
        /// EntityBase.Id represents FeedbackId
        /// </summary>
        public int InterviewerId { get; set; }

        public int StudentId { get; set; }

        public int InterviewRoomId { get; set; }

        public int Rating { get; set; }

        public string Comments { get; set; }

        /// <summary>
        /// AIAnalysis stored as JSON string
        /// </summary>
        public string AIAnalysis { get; set; }
    }
}

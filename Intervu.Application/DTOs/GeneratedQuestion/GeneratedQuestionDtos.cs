using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Entities.Constants.QuestionConstants;
using System;
using System.Collections.Generic;

namespace Intervu.Application.DTOs.GeneratedQuestion
{
    public class GeneratedQuestionDto
    {
        public Guid Id { get; set; }
        public Guid InterviewRoomId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public GeneratedQuestionStatus Status { get; set; }
    }

    public class ApproveGeneratedQuestionRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public ExperienceLevel Level { get; set; }
        public InterviewRound Round { get; set; }
        public QuestionCategory Category { get; set; }
        public Guid? InterviewExperienceId { get; set; }
        public List<Guid> CompanyIds { get; set; } = new();
        public List<Role> Roles { get; set; } = new();
        public List<Guid>? TagIds { get; set; } = new();
    }
}

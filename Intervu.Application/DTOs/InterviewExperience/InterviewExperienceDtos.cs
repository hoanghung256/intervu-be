using Intervu.Application.DTOs.Question;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Entities.Constants.QuestionConstants;
using System;
using System.Collections.Generic;

namespace Intervu.Application.DTOs.InterviewExperience
{
    public class InterviewExperienceFilterRequest
    {
        public string? SearchTerm { get; set; }
        public Guid? CompanyId { get; set; }
        public Role? Role { get; set; }
        public ExperienceLevel? Level { get; set; }
        public InterviewRound? LastRoundCompleted { get; set; }
        public SortOption? SortBy { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class CreateInterviewExperienceRequest
    {
        public Guid CompanyId { get; set; }
        public string Role { get; set; } = null!;
        public ExperienceLevel? Level { get; set; }
        public string LastRoundCompleted { get; set; } = null!;
        public string InterviewProcess { get; set; } = null!;
        public bool IsInterestedInContact { get; set; } = false;
        public List<CreateQuestionRequest> Questions { get; set; } = new();
    }

    public class UpdateInterviewExperienceRequest
    {
        public Guid CompanyId { get; set; }
        public string Role { get; set; } = null!;
        public ExperienceLevel? Level { get; set; }
        public string LastRoundCompleted { get; set; } = null!;
        public string InterviewProcess { get; set; } = null!;
        public bool IsInterestedInContact { get; set; }
    }

    public class InterviewExperienceSummaryDto
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public ExperienceLevel? Level { get; set; }
        public string LastRoundCompleted { get; set; } = string.Empty;
        public Guid ContributorId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int QuestionCount { get; set; }
    }

    public class InterviewExperienceDetailDto : InterviewExperienceSummaryDto
    {
        public string InterviewProcess { get; set; } = string.Empty;
        public bool IsInterestedInContact { get; set; }
        public List<QuestionDto> Questions { get; set; } = new();
    }
}


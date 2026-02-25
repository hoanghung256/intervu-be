using System;
using System.Collections.Generic;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.DTOs.QuestionBank
{
    public class QuestionResponseDTO
    {
        public Guid Id { get; set; }
        public Guid? ParentQuestionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid? CategoryId { get; set; }
        // public string? CategoryName { get; set; }
        public Guid? CompanyId { get; set; }
        // public string? CompanyName { get; set; }
        // public string? CompanyLogoPath { get; set; }
        // public int SaveCount { get; set; }
        // public int IWasAskedThisCount { get; set; }
        // public ExperienceLevel? Level { get; set; }
        public QuestionStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorProfilePicture { get; set; }
        public bool IsCodingQuestion { get; set; }
        public bool IsComment { get; set; }

        // Recursive children (comments/answers)
        public List<QuestionResponseDTO> ChildQuestions { get; set; } = new();
    }
}

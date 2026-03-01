using Intervu.Application.DTOs.Comment;
using Intervu.Domain.Entities.Constants;
using System;
using System.Collections.Generic;

namespace Intervu.Application.DTOs.Question
{
    public class QuestionFilterRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public string? QuestionType { get; set; }
        public string? Role { get; set; }
        public ExperienceLevel? Level { get; set; }
    }

    public class CreateQuestionRequest
    {
        public string QuestionType { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? Answer { get; set; }
    }

    public class UpdateQuestionRequest
    {
        public string QuestionType { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? Answer { get; set; }
    }

    public class QuestionDto
    {
        public Guid Id { get; set; }
        public string QuestionType { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Answer { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<CommentDto> Comments { get; set; } = new();
    }

    public class QuestionListItemDto
    {
        public Guid Id { get; set; }
        public string QuestionType { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Answer { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public ExperienceLevel? Level { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CommentCount { get; set; }
        public List<CommentDto> Comments { get; set; } = new();
    }

    public class RelatedQuestionDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class QuestionDetailDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public ExperienceLevel? Level { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorProfilePicture { get; set; }
        public string AuthorSlug { get; set; } = string.Empty;
        public int SaveCount { get; set; } = 0;
        public int IWasAskedThisCount { get; set; } = 0;
        public int CommentCount { get; set; }
        public List<CommentDetailDto> Comments { get; set; } = new();
        public List<RelatedQuestionDto> RelatedQuestions { get; set; } = new();
    }
}

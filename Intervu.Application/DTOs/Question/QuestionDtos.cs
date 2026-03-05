using Intervu.Application.DTOs.Comment;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Entities.Constants.QuestionConstants;
using System;
using System.Collections.Generic;

namespace Intervu.Application.DTOs.Question
{
    public class QuestionFilterRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? TagId { get; set; }
        public QuestionCategory? Category { get; set; }
        public Role? Role { get; set; }
        public ExperienceLevel? Level { get; set; }
        public InterviewRound? Round { get; set; }
        public SortOption? SortBy { get; set; }
    }

    public class CreateQuestionRequest
    {
        public string Title { get; set; } = null!;
        public string? Content { get; set; } = null!;
        public ExperienceLevel Level { get; set; }
        public InterviewRound Round { get; set; }
        public List<Guid> CompanyIds { get; set; } = new();
        public List<Role> Roles { get; set; } = new();
        public List<Guid>? TagIds { get; set; } = new();
        public QuestionCategory Category { get; set; }
        public string? Answer { get; set; }
        public Guid? LinkedQuestionId { get; set; }
    }

    public class AddQuestionResult
    {
        public Guid QuestionId { get; set; }
        public bool IsLinked { get; set; }
    }

    public class QuestionSearchResultDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<string> CompanyNames { get; set; } = new();
        public List<string> Roles { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        public int AnswerCount { get; set; }
    }

    public class UpdateQuestionRequest
    {
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public ExperienceLevel Level { get; set; }
        public InterviewRound Round { get; set; }
        public List<Guid> CompanyIds { get; set; } = new();
        public List<Role> Roles { get; set; } = new();
        public List<Guid> TagIds { get; set; } = new();
        public QuestionCategory Category { get; set; }
    }

    public class QuestionDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public ExperienceLevel Level { get; set; }
        public InterviewRound Round { get; set; }
        public QuestionStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<CommentDto> Comments { get; set; } = new();
    }

    public class TagDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class AnswerPreviewDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public int Upvotes { get; set; }
        public bool IsVerified { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorProfilePicture { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AnswerDetailDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public int Upvotes { get; set; }
        public bool IsVerified { get; set; }
        public Guid AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorProfilePicture { get; set; }
        public string AuthorSlug { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateAnswerRequest
    {
        public string Content { get; set; } = null!;
    }

    public class QuestionListItemDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public ExperienceLevel Level { get; set; }
        public InterviewRound Round { get; set; }
        public QuestionStatus Status { get; set; }
        public int ViewCount { get; set; }
        public int SaveCount { get; set; }
        public int AnswerCount { get; set; }
        public bool IsHot { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> CompanyNames { get; set; } = new();
        public List<string> Roles { get; set; } = new();
        public List<TagDto> Tags { get; set; } = new();
        public string Category { get; set; } = string.Empty;
        public AnswerPreviewDto? TopAnswer { get; set; }
    }

    public class RelatedQuestionDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<string> CompanyNames { get; set; } = new();
        public List<string> Roles { get; set; } = new();
        public int AnswerCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class QuestionDetailDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public ExperienceLevel Level { get; set; }
        public InterviewRound Round { get; set; }
        public QuestionStatus Status { get; set; }
        public int ViewCount { get; set; }
        public int SaveCount { get; set; }
        public int AnswerCount { get; set; }
        public bool IsHot { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorProfilePicture { get; set; }
        public string AuthorSlug { get; set; } = string.Empty;
        public List<string> CompanyNames { get; set; } = new();
        public List<string> Roles { get; set; } = new();
        public List<TagDto> Tags { get; set; } = new();
        public string Category { get; set; } = string.Empty;
        public List<AnswerDetailDto> Answers { get; set; } = new();
        public List<CommentDetailDto> Comments { get; set; } = new();
        public List<RelatedQuestionDto> RelatedQuestions { get; set; } = new();
    }
}

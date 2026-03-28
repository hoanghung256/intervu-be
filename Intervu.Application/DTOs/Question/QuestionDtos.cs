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
        public int Vote { get; set; }
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
        public Guid? LinkedQuestionId { get; set; }
        public string? Answer { get; set; }
    }

    public class AddQuestionResult
    {
        public Guid QuestionId { get; set; }
        public bool IsLinked { get; set; }
    }

    public class ReportQuestionRequest
    {
        public string Reason { get; set; } = string.Empty;
    }

    public class ReportQuestionResult
    {
        public Guid ReportId { get; set; }
    }

    public class QuestionSearchResultDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<string> CompanyNames { get; set; } = new();
        public List<string> Roles { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        public int Vote { get; set; }
        public int CommentCount { get; set; }
    }

    public class UpdateQuestionRequest
    {
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public ExperienceLevel Level { get; set; }
        public InterviewRound Round { get; set; }
        public List<Guid> CompanyIds { get; set; } = new();
        public List<string> CompanyNames { get; set; } = new();
        public List<Role> Roles { get; set; } = new();
        public List<Guid> TagIds { get; set; } = new();
        public QuestionCategory Category { get; set; }
    }

    public class QuestionDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int Vote { get; set; }
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
        public int CommentCount { get; set; }
        public int Vote { get; set; }
        public bool IsHot { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> CompanyNames { get; set; } = new();
        public List<string> Roles { get; set; } = new();
        public List<TagDto> Tags { get; set; } = new();
        public string Category { get; set; } = string.Empty;
        public bool IsLikedByUser { get; set; }
        public bool IsSavedByUser { get; set; }
    }

    public class QuestionBankItemDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public class RelatedQuestionDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<string> CompanyNames { get; set; } = new();
        public List<string> Roles { get; set; } = new();
        public int CommentCount { get; set; }
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
        public int CommentCount { get; set; }
        public bool IsHot { get; set; }
        public int Vote { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorProfilePicture { get; set; }
        public string AuthorSlug { get; set; } = string.Empty;
        public List<string> CompanyNames { get; set; } = new();
        public List<string> Roles { get; set; } = new();
        public List<TagDto> Tags { get; set; } = new();
        public string Category { get; set; } = string.Empty;
        public List<CommentDetailDto> Comments { get; set; } = new();
        public List<RelatedQuestionDto> RelatedQuestions { get; set; } = new();
        public bool IsLikedByUser { get; set; }
        public bool IsSavedByUser { get; set; }
    }

    public class QuestionReportFilterRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public QuestionReportStatus? Status { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class QuestionReportItemDto
    {
        public Guid Id { get; set; }
        public Guid QuestionId { get; set; }
        public string QuestionTitle { get; set; } = string.Empty;
        public Guid ReporterId { get; set; }
        public string ReporterName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public QuestionReportStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class UpdateQuestionReportStatusRequest
    {
        public QuestionReportStatus Status { get; set; }
    }
}

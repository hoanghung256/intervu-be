using Intervu.Domain.Entities.Constants.PreparedQuestionConstants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Intervu.Application.DTOs.PreparedQuestion
{
    public class PreparedQuestionDto
    {
        public Guid Id { get; set; }
        public Guid InterviewRoomId { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid? SourceBankQuestionId { get; set; }

        public PreparedQuestionInteractionType InteractionType { get; set; }
        public string? DisplayCategoryLabel { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public string? FunctionName { get; set; }
        public object[]? TestCases { get; set; }

        public PreparedQuestionStatus Status { get; set; }
        public DateTime? AskedAt { get; set; }
        public int SortOrder { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Kept for API compatibility; always true (coach may send coding prompts without test cases).
        /// </summary>
        public bool IsReadyForEditor { get; set; }
    }

    public class CreateCustomPreparedQuestionRequest
    {
        [Required]
        public PreparedQuestionInteractionType InteractionType { get; set; }

        [Required]
        [MaxLength(500)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? DisplayCategoryLabel { get; set; }

        [MaxLength(200)]
        public string? FunctionName { get; set; }

        public object[]? TestCases { get; set; }
    }

    public class ImportBankQuestionRequest
    {
        [Required]
        public Guid BankQuestionId { get; set; }
    }

    public class UpdatePreparedQuestionRequest
    {
        [Required]
        [MaxLength(500)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? DisplayCategoryLabel { get; set; }

        [MaxLength(200)]
        public string? FunctionName { get; set; }

        public object[]? TestCases { get; set; }
    }

    public class ReorderPreparedQuestionsRequest
    {
        [Required]
        public List<Guid> OrderedIds { get; set; } = new();
    }
}

using System;
using System.Collections.Generic;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.DTOs.QuestionBank
{
    public class CreateQuestionRequestDTO
    {
        public Guid? ParentQuestionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid? CategoryId { get; set; }
        public Guid? CompanyId { get; set; }
        // public ExperienceLevel? Level { get; set; }
        public bool IsCodingQuestion { get; set; }
        public bool IsComment { get; set; }
        public string? ProgrammingLanguage { get; set; }
        public string? InitialCode { get; set; }
    }
}

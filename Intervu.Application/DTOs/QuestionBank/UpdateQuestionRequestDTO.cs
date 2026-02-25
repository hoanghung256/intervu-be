using System;
using System.Collections.Generic;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.DTOs.QuestionBank
{
    public class UpdateQuestionRequestDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid? CategoryId { get; set; }
        public Guid? CompanyId { get; set; }
        // public ExperienceLevel? Level { get; set; }
        public QuestionStatus Status { get; set; }
    }
}

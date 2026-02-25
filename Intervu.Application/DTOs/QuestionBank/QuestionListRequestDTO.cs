using System;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.DTOs.QuestionBank
{
    public class QuestionListRequestDTO
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? CompanyId { get; set; }
         public ExperienceLevel? Level { get; set; }
        public bool OnlyApproved { get; set; } = true;
        public bool? IsComment { get; set; } 
    }
}

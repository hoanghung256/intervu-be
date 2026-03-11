using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Domain.Entities
{
    public class CodingQuestionAdditional
    {
        public Guid QuestionId { get; set; }
        public Question Question { get; set; }
        public string ProgramingLanguage { get; set; } = null!;
        public string InitialCode { get; set; } = null!;
    }
}

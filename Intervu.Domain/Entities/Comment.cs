using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Domain.Entities
{
    public class Comment
    {
        public Guid Id { get; set; }
        public Guid QuestionId { get; set; }
        public Question Question { get; set; } = null!;
        public string Content { get; set; } = null!;
        public int Vote { get; set; } = 0;
        public bool IsAnswer { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdateAt { get; set; } = DateTime.UtcNow;
        public Guid CreateBy { get; set; }
        public Guid UpdateBy { get; set; }
    }
}

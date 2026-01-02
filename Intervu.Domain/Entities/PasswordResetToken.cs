using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Domain.Abstractions.Entity;

namespace Intervu.Domain.Entities
{
    public class PasswordResetToken : EntityBase<Guid>
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public DateTime CreatedAt { get; set; }
        public User User { get; set; }
    }
}

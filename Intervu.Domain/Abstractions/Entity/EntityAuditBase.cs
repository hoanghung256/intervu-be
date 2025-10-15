using Intervu.Domain.Abstractions.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Domain.Abstractions.Entities
{
    /*
     * For entity that requires both date tracking and soft delete functionality.
     */
    public abstract class EntityAuditBase<T> : EntityBase<T>, IAuditable
    {
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}

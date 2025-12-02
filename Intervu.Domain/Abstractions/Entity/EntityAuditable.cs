using Intervu.Domain.Abstractions.Entity.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Domain.Abstractions.Entity
{
    /*
    * For entity that requires both date tracking and soft delete functionality.
    */
    public abstract class EntityAuditable<T> : EntityBase<T>, IAuditable
    {
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}

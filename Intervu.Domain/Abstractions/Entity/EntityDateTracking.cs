using Intervu.Domain.Abstractions.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Domain.Abstractions.Entities
{
    /*
    * For entity that requires date tracking functionality only.
    */
    public abstract class EntityDateTracking<T> : EntityBase<T>, IDateTracking
    {
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

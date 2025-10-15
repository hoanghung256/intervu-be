using Intervu.Domain.Abstractions.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Domain.Abstractions.Entities
{
    public abstract class EntityBase<T> : IEntityBase<T>
    {
        public T Id { get; set; }
    }
}

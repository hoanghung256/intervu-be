using Intervu.Domain.Abstractions.Entity;

namespace Intervu.Domain.Entities
{
    public class PlatformSetting : EntityDateTracking<Guid>
    {
        public decimal CommissionRate { get; set; }
    }
}

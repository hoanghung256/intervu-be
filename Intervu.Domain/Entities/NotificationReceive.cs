using System;

namespace Intervu.Domain.Entities
{
    public class NotificationReceive
    {
        /// <summary>
        /// Composite key: NotificationId + ReceiverId
        /// </summary>
        public Guid NotificationId { get; set; }

        public Guid ReceiverId { get; set; }
    }
}

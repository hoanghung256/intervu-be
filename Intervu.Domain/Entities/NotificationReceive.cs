using System;

namespace Intervu.Domain.Entities
{
    public class NotificationReceive
    {
        /// <summary>
        /// Composite key: NotificationId + ReceiverId
        /// </summary>
        public int NotificationId { get; set; }

        public int ReceiverId { get; set; }
    }
}

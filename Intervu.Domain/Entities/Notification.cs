using Intervu.Domain.Abstractions.Entities;
using System;

namespace Intervu.Domain.Entities
{
    public class Notification : EntityBase<int>
    {
        /// <summary>
        /// EntityBase.Id represents Notification.Id
        /// </summary>
        public string Title { get; set; }

        public string Message { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}

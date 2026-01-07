using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.DTOs.Feedback
{
    public class GetFeedbackRequest
    {
        public Guid? FeedbackId { get; set; }

        public Guid? StudentId { get; set; }

        public int Rating { get; set; }

        public string Comments { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Domain.Entities.Constants
{
    public enum InterviewTypeStatus
    {
        Draft = 0,      // Created but not visible to coaches
        Active = 1,     // Coaches can enable it
        Inactive = 2,   // Hidden from coaches & candidates
        Deprecated = 3 // Old type, kept for history
    }
}

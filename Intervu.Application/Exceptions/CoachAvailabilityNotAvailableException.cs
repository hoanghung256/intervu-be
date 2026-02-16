using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.Exceptions
{
    public sealed class CoachAvailabilityNotAvailableException(string message) : BusinessException(message, StatusCodes.Status409Conflict)
    {
    }
}

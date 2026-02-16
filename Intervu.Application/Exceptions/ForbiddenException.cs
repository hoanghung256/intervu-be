using Microsoft.AspNetCore.Http;

namespace Intervu.Application.Exceptions
{
    public sealed class ForbiddenException(string message) : BusinessException(message, StatusCodes.Status403Forbidden)
    {
    }
}

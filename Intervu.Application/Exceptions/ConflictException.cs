using Microsoft.AspNetCore.Http;

namespace Intervu.Application.Exceptions
{
    public sealed class ConflictException(string message) : BusinessException(message, StatusCodes.Status409Conflict)
    {
    }
}

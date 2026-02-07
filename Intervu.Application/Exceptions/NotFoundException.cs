using Microsoft.AspNetCore.Http;

namespace Intervu.Application.Exceptions
{
    public sealed class NotFoundException(string message) : BusinessException(message, StatusCodes.Status404NotFound)
    {
    }
}

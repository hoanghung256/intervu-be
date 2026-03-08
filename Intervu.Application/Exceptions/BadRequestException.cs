using Microsoft.AspNetCore.Http;

namespace Intervu.Application.Exceptions
{
    public sealed class BadRequestException(string message) : BusinessException(message, StatusCodes.Status400BadRequest)
    {
    }
}

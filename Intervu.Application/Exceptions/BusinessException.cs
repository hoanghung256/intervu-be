using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.Exceptions
{
    public abstract class BusinessException(string message, int statusCode) : Exception(message)
    {
        public int StatusCode { get; } = statusCode;
    }
}

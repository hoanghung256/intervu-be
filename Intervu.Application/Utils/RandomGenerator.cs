using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.Utils
{
    internal class RandomGenerator
    {
        internal static int GenerateOrderCode()
        {
            return (int)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 1000000000);
        }
    }
}

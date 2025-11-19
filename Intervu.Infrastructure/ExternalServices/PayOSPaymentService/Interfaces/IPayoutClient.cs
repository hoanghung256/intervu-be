using PayOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Infrastructure.ExternalServices.PayOSPaymentService.Interfaces
{
    internal interface IPayoutClient
    {
        PayOSClient Client { get; }
    }
}

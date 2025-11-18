using Intervu.Infrastructure.ExternalServices.PayOSPaymentService.Interfaces;
using PayOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Infrastructure.ExternalServices.PayOSPaymentService
{
    public class PayoutClient(PayOSOptions options) : IPayoutClient
    {
        public PayOSClient Client { get; } = new PayOSClient(options);
    }
}

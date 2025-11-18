using Intervu.Infrastructure.ExternalServices.PayOSPaymentService.Interfaces;
using PayOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Infrastructure.ExternalServices.PayOSPaymentService
{
    public class PaymentClient(PayOSOptions options) : IPaymentClient
    {
        public PayOSClient Client { get; } = new PayOSClient(options);
    }
}

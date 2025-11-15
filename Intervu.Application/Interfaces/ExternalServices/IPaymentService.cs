using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.ExternalServices
{
    public interface IPaymentService
    {
        Task<string> CreatePaymentOrderAsync(int ammount, string description);

        Task<bool> VerifyPaymentAsync(string paymentId, string orderId, string signature);

        Task<bool> CreateSpendOrderAsync(int amount, string description, string targetBankId, string targetBankAccountNumber);
    }
}

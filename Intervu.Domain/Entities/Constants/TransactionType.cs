using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Domain.Entities.Constants
{
    /// <summary>
    /// Represents supported transaction directions and purposes in the wallet/payment flow.
    /// </summary>
    public enum TransactionType
    {
        /// <summary>
        /// Money paid by a user (for example: paying for a booking or service).
        /// </summary>
        Payment,

        /// <summary>
        /// Money sent out by the platform to a recipient (for example: coach payout).
        /// </summary>
        Payout,

        /// <summary>
        /// Money returned to a user after a previous payment.
        /// </summary>
        Refund,

        /// <summary>
        /// Income credited to a user account from completed services.
        /// This increases the internal available balance and represents earned funds
        /// before a separate withdrawal or payout transaction moves money out.
        /// </summary>
        Earnings,

        /// <summary>
        /// Money taken out from an account balance to an external destination.
        /// </summary>
        Withdrawal
    }
}

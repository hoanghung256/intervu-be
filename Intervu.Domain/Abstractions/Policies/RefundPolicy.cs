using Intervu.Domain.Abstractions.Policies.Interfaces;

namespace Intervu.Domain.Abstractions.Policies
{
    public class RefundPolicy : IRefundPolicy
    {
        public int CalculateRefundAmount(int paidAmount, DateTime interviewStartTime, DateTime cancelledAt)
        {
            var hoursBeforeInterview =
             (interviewStartTime - cancelledAt).TotalHours;

            if (hoursBeforeInterview >= 24)
                return paidAmount;

            if (hoursBeforeInterview >= 12)
                return paidAmount / 2;

            return 0;
        }
    }
}

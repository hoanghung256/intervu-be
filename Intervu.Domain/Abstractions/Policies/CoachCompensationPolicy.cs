using System;
using Intervu.Domain.Abstractions.Policies.Interfaces;

namespace Intervu.Domain.Abstractions.Policies
{
    public class CoachCompensationPolicy : ICoachCompensationPolicy
    {
        private readonly IRefundPolicy _refundPolicy;

        public CoachCompensationPolicy(IRefundPolicy refundPolicy)
        {
            _refundPolicy = refundPolicy;
        }

        public int CalculateCompensationAmount(int paidAmount, DateTime interviewStartTime, DateTime cancelledAt)
        {
            var refundAmount = _refundPolicy.CalculateRefundAmount(paidAmount, interviewStartTime, cancelledAt);
            return Math.Max(paidAmount - refundAmount, 0);
        }
    }
}

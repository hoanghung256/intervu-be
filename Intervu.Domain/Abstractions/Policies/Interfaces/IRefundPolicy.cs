namespace Intervu.Domain.Abstractions.Policies.Interfaces
{
    public interface IRefundPolicy
    {
        int CalculateRefundAmount(int paidAmount, DateTime interviewStartTime, DateTime cancelledAt);
    }
}

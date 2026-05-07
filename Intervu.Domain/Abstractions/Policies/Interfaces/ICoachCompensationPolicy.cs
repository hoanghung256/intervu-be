namespace Intervu.Domain.Abstractions.Policies.Interfaces
{
    public interface ICoachCompensationPolicy
    {
        int CalculateCompensationAmount(int paidAmount, DateTime interviewStartTime, DateTime cancelledAt);
    }
}

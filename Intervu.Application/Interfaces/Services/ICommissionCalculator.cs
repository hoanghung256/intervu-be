namespace Intervu.Application.Interfaces.Services
{
    public interface ICommissionCalculator
    {
        Task<CommissionBreakdown> ComputeAsync(int gross);
    }

    public record CommissionBreakdown(int Net, int Commission, decimal Rate);
}

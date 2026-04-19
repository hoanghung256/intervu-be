namespace Intervu.Application.Interfaces.UseCases.Admin
{
    public interface IUpdateCommissionRate
    {
        Task<decimal> ExecuteAsync(decimal rate);
    }
}

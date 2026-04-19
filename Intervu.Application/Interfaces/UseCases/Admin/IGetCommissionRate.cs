namespace Intervu.Application.Interfaces.UseCases.Admin
{
    public interface IGetCommissionRate
    {
        Task<decimal> ExecuteAsync();
    }
}

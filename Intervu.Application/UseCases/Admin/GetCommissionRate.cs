using Intervu.Application.Interfaces.UseCases.Admin;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.Admin
{
    public class GetCommissionRate : IGetCommissionRate
    {
        private const decimal DefaultRate = 0.30m;
        private readonly IPlatformSettingRepository _repo;

        public GetCommissionRate(IPlatformSettingRepository repo) => _repo = repo;

        public async Task<decimal> ExecuteAsync()
        {
            var s = await _repo.GetCurrentAsync();
            return s?.CommissionRate ?? DefaultRate;
        }
    }
}

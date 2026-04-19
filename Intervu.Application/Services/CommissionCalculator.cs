using Intervu.Application.Interfaces.Services;
using Intervu.Domain.Repositories;

namespace Intervu.Application.Services
{
    public class CommissionCalculator : ICommissionCalculator
    {
        private const decimal DefaultRate = 0.30m;
        private readonly IPlatformSettingRepository _platformSettingRepository;

        public CommissionCalculator(IPlatformSettingRepository platformSettingRepository)
        {
            _platformSettingRepository = platformSettingRepository;
        }

        public async Task<CommissionBreakdown> ComputeAsync(int gross)
        {
            var setting = await _platformSettingRepository.GetCurrentAsync();
            var rate = setting?.CommissionRate ?? DefaultRate;
            var commission = (int)Math.Round(gross * rate, MidpointRounding.AwayFromZero);
            var net = gross - commission;
            return new CommissionBreakdown(net, commission, rate);
        }
    }
}

using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.UseCases.Admin;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.Admin
{
    public class UpdateCommissionRate : IUpdateCommissionRate
    {
        private readonly IPlatformSettingRepository _repo;

        public UpdateCommissionRate(IPlatformSettingRepository repo) => _repo = repo;

        public async Task<decimal> ExecuteAsync(decimal rate)
        {
            if (rate < 0m || rate >= 1m)
                throw new BadRequestException("Commission rate must be between 0 (inclusive) and 1 (exclusive).");

            var setting = await _repo.GetCurrentAsync();
            if (setting == null)
            {
                setting = new PlatformSetting
                {
                    Id = Guid.NewGuid(),
                    CommissionRate = rate,
                    CreatedAt = DateTime.UtcNow
                };
                await _repo.AddAsync(setting);
            }
            else
            {
                setting.CommissionRate = rate;
                setting.UpdatedAt = DateTime.UtcNow;
                _repo.UpdateAsync(setting);
            }

            await _repo.SaveChangesAsync();
            return setting.CommissionRate;
        }
    }
}

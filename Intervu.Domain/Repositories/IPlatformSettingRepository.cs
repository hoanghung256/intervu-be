using Intervu.Domain.Entities;

namespace Intervu.Domain.Repositories
{
    public interface IPlatformSettingRepository : IRepositoryBase<PlatformSetting>
    {
        Task<PlatformSetting?> GetCurrentAsync();
    }
}

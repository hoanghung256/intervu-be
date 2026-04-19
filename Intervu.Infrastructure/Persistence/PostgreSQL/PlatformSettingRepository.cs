using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class PlatformSettingRepository(IntervuPostgreDbContext context)
        : RepositoryBase<PlatformSetting>(context), IPlatformSettingRepository
    {
        public async Task<PlatformSetting?> GetCurrentAsync()
        {
            return await _context.PlatformSettings
                .OrderBy(x => x.CreatedAt)
                .FirstOrDefaultAsync();
        }
    }
}

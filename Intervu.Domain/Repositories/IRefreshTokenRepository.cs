using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Domain.Entities;

namespace Intervu.Domain.Repositories
{
    public interface IRefreshTokenRepository : IRepositoryBase<RefreshToken>
    {
        Task<string> CreateRefreshTokenAsync(Guid userId);
        Task<RefreshToken?> GetValidTokenAsync(string token);
        Task RevokeAllUserTokensAsync(Guid userId);
        Task RevokeTokenAsync(string token);
        Task DeleteExpiredTokensAsync();
    }
}

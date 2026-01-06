using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Domain.Entities;

namespace Intervu.Domain.Repositories
{
    public interface IPasswordResetTokenRepository : IRepositoryBase<PasswordResetToken>
    {
        Task<PasswordResetToken> GetValidTokenAsync(string tokenHash);
        Task<PasswordResetToken> CreateTokenAsync(Guid userId, string tokenHash, DateTime expiresAt);
        Task<bool> MarkAsUsedAsync(Guid tokenId);
        Task InvalidateAllUserTokensAsync(Guid userId);
        Task<PasswordResetToken?> GetLatestValidTokenByUserIdAsync(Guid userId);
    }
}

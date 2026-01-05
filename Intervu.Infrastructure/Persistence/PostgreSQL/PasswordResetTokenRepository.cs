using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Intervu.Infrastructure.Persistence.SqlServer.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class PasswordResetTokenRepository : RepositoryBase<PasswordResetToken>, IPasswordResetTokenRepository
    {
        public PasswordResetTokenRepository(IntervuPostgreDbContext context) : base(context)
        {
        }
        public Task AddAsync(PasswordResetToken entity)
        {
            throw new NotImplementedException();
        }

        public async Task<PasswordResetToken> CreateTokenAsync(Guid userId, string tokenHash, DateTime expiresAt)
        {
            var token = new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = tokenHash,
                ExpiresAt = expiresAt,
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            await _context.PasswordResetTokens.AddAsync(token);
            await _context.SaveChangesAsync();

            return token;
        }

        public void DeleteAsync(PasswordResetToken entity)
        {
            throw new NotImplementedException();
        }

        public Task<PasswordResetToken?> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<PasswordResetToken?> GetLatestValidTokenByUserIdAsync(Guid userId)
        {
            var now = DateTime.UtcNow;
            return await _context.PasswordResetTokens
                .Where(t => t.UserId == userId && !t.IsUsed && t.ExpiresAt > now)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<PasswordResetToken> GetValidTokenAsync(string tokenHash)
        {
            DateTime now = DateTime.UtcNow;

            return await _context.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == tokenHash &&
                    !t.IsUsed &&
                    t.ExpiresAt > now);
        }

        public async Task InvalidateAllUserTokensAsync(Guid userId)
        {
            var tokens = await _context.PasswordResetTokens
                .Where(t => t.UserId == userId && !t.IsUsed)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsUsed = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> MarkAsUsedAsync(Guid tokenId)
        {
            var token = await _context.PasswordResetTokens.FindAsync(tokenId);

            if (token == null || token.IsUsed)
            {
                return false;
            }

            token.IsUsed = true;
            await _context.SaveChangesAsync();

            return true;
        }

        public Task<int> SaveChangesAsync()
        {
            throw new NotImplementedException();
        }

        public void SoftDeleteAsync(PasswordResetToken entity)
        {
            throw new NotImplementedException();
        }

        public void UpdateAsync(PasswordResetToken entity)
        {
            throw new NotImplementedException();
        }
    }
}

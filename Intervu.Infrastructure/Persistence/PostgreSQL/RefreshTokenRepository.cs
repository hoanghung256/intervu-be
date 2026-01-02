using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class RefreshTokenRepository : RepositoryBase<RefreshToken>, IRefreshTokenRepository
    {
        private readonly IConfiguration _configuration;

        public RefreshTokenRepository(IntervuPostgreDbContext context, IConfiguration configuration) : base(context)
        {
            _configuration = configuration;
        }

        public async Task<string> CreateRefreshTokenAsync(Guid userId)
        {
            var bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            string token = Convert.ToBase64String(bytes);

            var refreshTokenValidityDays = _configuration.GetValue<int>("JwtConfig:RefreshTokenValidityInDays");
            if (refreshTokenValidityDays <= 0) refreshTokenValidityDays = 7; // Default 7 ngày nếu không config

            RefreshToken refreshToken = new RefreshToken
            { 
                UserId = userId,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenValidityDays),
                CreatedAt = DateTime.UtcNow,
            };

            await _context.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            return token;

        }

        public async Task DeleteExpiredTokensAsync()
        {
            var expiredTokens = await _context.RefreshTokens.Where(rt => rt.ExpiresAt <= DateTime.UtcNow).ToListAsync();
            _context.RefreshTokens.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetValidTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token && rt.ExpiresAt > DateTime.UtcNow);
        }

        public async Task RevokeAllUserTokensAsync(Guid userId)
        {
            var tokens = await _context.RefreshTokens.Where(rt => rt.UserId == userId).ToListAsync();

            _context.RefreshTokens.RemoveRange(tokens);
            await _context.SaveChangesAsync();
        }

        public async Task RevokeTokenAsync(string token)
        {
            var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);

            if(refreshToken != null)
            {
                _context.RefreshTokens.Remove(refreshToken);
                await _context.SaveChangesAsync();
            }
        }
    }
}

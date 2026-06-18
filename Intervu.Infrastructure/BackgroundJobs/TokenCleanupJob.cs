using Hangfire;
using Intervu.Application.Interfaces.BackgroundJobs;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Intervu.Infrastructure.BackgroundJobs
{
    public class TokenCleanupJob : IRecurringJob
    {
        private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ILogger<TokenCleanupJob> _logger;

        public TokenCleanupJob(
            IPasswordResetTokenRepository passwordResetTokenRepository,
            IRefreshTokenRepository refreshTokenRepository,
            ILogger<TokenCleanupJob> logger)
        {
            _passwordResetTokenRepository = passwordResetTokenRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _logger = logger;
        }

        public string JobId => "TokenCleanup";
        public string CronExpression => Cron.Daily();

        public async Task ExecuteAsync()
        {
            await _passwordResetTokenRepository.DeleteExpiredTokensAsync();
            await _refreshTokenRepository.DeleteExpiredTokensAsync();
            _logger.LogInformation("Expired tokens cleanup completed");
        }
    }
}

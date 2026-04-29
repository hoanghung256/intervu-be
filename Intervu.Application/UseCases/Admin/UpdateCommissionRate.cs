using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.ExternalServices.Email;
using Intervu.Application.Interfaces.UseCases.Admin;
using Intervu.Application.Interfaces.UseCases.Notification;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Configuration;

namespace Intervu.Application.UseCases.Admin
{
    public class UpdateCommissionRate : IUpdateCommissionRate
    {
        private readonly IPlatformSettingRepository _repo;
        private readonly IBackgroundService _backgroundService;
        private readonly IConfiguration _configuration;

        public UpdateCommissionRate(
            IPlatformSettingRepository repo,
            IBackgroundService backgroundService,
            IConfiguration configuration)
        {
            _repo = repo;
            _backgroundService = backgroundService;
            _configuration = configuration;
        }

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

            // Broadcast in-app notification to all Coaches in background (non-blocking)
            var ratePercent = Math.Round(rate * 100, 2);
            var title = "Platform Commission Rate Updated";
            var message = $"The platform commission rate has been updated to {ratePercent}%. This applies to all future bookings.";

            _backgroundService.Enqueue<INotificationUseCase>(uc =>
                uc.BroadcastToRoleAsync(
                    UserRole.Coach.ToString(),
                    NotificationType.SystemAnnouncement,
                    title,
                    message,
                    null));

            // Broadcast email to all Coaches in background (paginated, non-blocking)
            var effectiveDate = DateTime.UtcNow.ToString("dd/MM/yyyy");
            var frontEndUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:5173";
            var dashboardLink = $"{frontEndUrl}/wallet";

            var emailPlaceholders = new Dictionary<string, string>
            {
                ["CommissionRate"] = ratePercent.ToString(),
                ["EffectiveDate"] = effectiveDate,
                ["DashboardLink"] = dashboardLink
                // CoachName is injected per-user inside BroadcastEmailToRoleAsync
            };

            _backgroundService.Enqueue<IEmailService>(svc =>
                svc.BroadcastEmailToRoleAsync(
                    UserRole.Coach.ToString(),
                    "CommissionRateUpdated",
                    emailPlaceholders));

            return setting.CommissionRate;
        }
    }
}

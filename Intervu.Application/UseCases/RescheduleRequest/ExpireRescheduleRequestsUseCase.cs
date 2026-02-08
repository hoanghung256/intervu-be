using Intervu.Application.Interfaces.UseCases.RescheduleRequest;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Intervu.Application.UseCases.RescheduleRequest
{
    internal class ExpireRescheduleRequestsUseCase : IExpireRescheduleRequestsUseCase
    {
        private readonly ILogger<ExpireRescheduleRequestsUseCase> _logger;
        private readonly IRescheduleRequestRepository _rescheduleRequestRepository;

        public ExpireRescheduleRequestsUseCase(
            ILogger<ExpireRescheduleRequestsUseCase> logger,
            IRescheduleRequestRepository rescheduleRequestRepository)
        {
            _logger = logger;
            _rescheduleRequestRepository = rescheduleRequestRepository;
        }

        public async Task ExecuteAsync()
        {
            var expiredRequests = await _rescheduleRequestRepository.GetExpiredRequestsAsync();
            
            var expiredList = expiredRequests.ToList();
            if (!expiredList.Any())
            {
                _logger.LogInformation("No expired reschedule requests found");
                return;
            }

            foreach (var request in expiredList)
            {
                request.Status = RescheduleRequestStatus.Expired;
                _rescheduleRequestRepository.UpdateAsync(request);
            }

            await _rescheduleRequestRepository.SaveChangesAsync();

            _logger.LogInformation("Expired {Count} reschedule requests", expiredList.Count);
        }
    }
}

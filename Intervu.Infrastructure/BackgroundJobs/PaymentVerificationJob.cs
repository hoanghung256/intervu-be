using Intervu.Application.Interfaces.BackgroundJobs;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;
using Microsoft.Extensions.Logging;

namespace Intervu.Infrastructure.BackgroundJobs
{
    public class PaymentVerificationJob : IRecurringJob
    {
        private readonly IVerifyPendingPayments _verifyPendingPayments;
        private readonly ILogger<PaymentVerificationJob> _logger;

        public PaymentVerificationJob(
            IVerifyPendingPayments verifyPendingPayments,
            ILogger<PaymentVerificationJob> logger)
        {
            _verifyPendingPayments = verifyPendingPayments;
            _logger = logger;
        }

        public string JobId => "PaymentVerification";
        public string CronExpression => "*/5 * * * *";

        public async Task ExecuteAsync()
        {
            var count = await _verifyPendingPayments.ExecuteAsync();
            if (count > 0)
                _logger.LogInformation(
                    "PaymentVerificationJob: confirmed {Count} payment(s) missed by webhook",
                    count);
        }
    }
}

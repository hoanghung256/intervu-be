using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Intervu.Application.UseCases.InterviewBooking
{
    internal class CreateBookingCheckoutUrl : ICreateBookingCheckoutUrl
    {
        private readonly ILogger<CreateBookingCheckoutUrl> _logger;
        private readonly IPaymentService _paymentService;
        private readonly ICoachProfileRepository _coachProfileRepository;
        private readonly ITransactionRepository _transactionRepository;

        public CreateBookingCheckoutUrl(ILogger<CreateBookingCheckoutUrl> logger, IPaymentService paymentService, ICoachProfileRepository coachProfileRepository, ITransactionRepository transactionRepository) 
        {
            _logger = logger;
            _paymentService = paymentService;
            _coachProfileRepository = coachProfileRepository;
            _transactionRepository = transactionRepository;
        }

        public async Task<string?> ExecuteAsync(Guid candidateId, Guid coachId, Guid coachAvailabilityId, string returnUrl)
        {
            try
            {
                var coach = await _coachProfileRepository.GetProfileByIdAsync(coachId) ?? throw new Exception("Interviewer not found");
                Domain.Entities.InterviewBookingTransaction t = new()
                {
                    UserId = candidateId,
                    Amount = coach.CurrentAmount ?? 0,
                    Status = coach.CurrentAmount == 0 ? Domain.Entities.Constants.TransactionStatus.Paid : Domain.Entities.Constants.TransactionStatus.Created,
                    Type = Domain.Entities.Constants.TransactionType.Payment,
                    CoachAvailabilityId = coachAvailabilityId,
                };

                Domain.Entities.InterviewBookingTransaction t2 = new()
                {
                    UserId = coachId,
                    Amount = coach.CurrentAmount ?? 0,
                    Status = coach.CurrentAmount == 0 ? Domain.Entities.Constants.TransactionStatus.Paid : Domain.Entities.Constants.TransactionStatus.Created,
                    Type = Domain.Entities.Constants.TransactionType.Payout,
                    CoachAvailabilityId = coachAvailabilityId,
                };

                await _transactionRepository.AddAsync(t);
                await _transactionRepository.AddAsync(t2);
                await _transactionRepository.SaveChangesAsync();

                if (t.Amount == 0) return null;
            
                string? checkoutUrl = await _paymentService.CreatePaymentOrderAsync(
                    t.OrderCode,
                    t.Amount,
                    $"Book interview",
                    returnUrl
                );
                return checkoutUrl;
            } 
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create checkout URL");
                throw;
            }
        }
    }
}

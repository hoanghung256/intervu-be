using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;
using Intervu.Application.Utils;
using Intervu.Domain.Entities.Constants;
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
        private readonly ICoachAvailabilitiesRepository _coachAvailabilitiesRepository;
        private readonly IBackgroundService _jobService;

        public CreateBookingCheckoutUrl(ILogger<CreateBookingCheckoutUrl> logger, IPaymentService paymentService, ICoachProfileRepository coachProfileRepository, ITransactionRepository transactionRepository, ICoachAvailabilitiesRepository coachAvailabilitiesRepository, IBackgroundService jobService) 
        {
            _logger = logger;
            _paymentService = paymentService;
            _coachProfileRepository = coachProfileRepository;
            _transactionRepository = transactionRepository;
            _coachAvailabilitiesRepository = coachAvailabilitiesRepository;
            _jobService = jobService;
        }

        public async Task<string?> ExecuteAsync(Guid candidateId, Guid coachId, Guid coachAvailabilityId, string returnUrl)
        {
            try
            {
                var availability = await _coachAvailabilitiesRepository.GetByIdAsync(coachAvailabilityId) ?? throw new NotFoundException("Coach availability not found");

                if (availability.CoachId != coachId) throw new Exception("Coach availability does not belong to the specified coach");

                if (availability.Status != CoachAvailabilityStatus.Available) throw new CoachAvailabilityNotAvailableException("Coach availability is not available for booking");

                // Reserve the slot for booking user
                availability.Status = CoachAvailabilityStatus.Reserved;
                availability.ReservingForUserId = candidateId;

                // Auto expired reserve after 5mins
                _jobService.Schedule<ICoachAvailabilitiesRepository>(
                    repo => repo.ExpireReservedSlot(coachAvailabilityId, candidateId),
                    TimeSpan.FromMinutes(5)
                );

                var coach = await _coachProfileRepository.GetProfileByIdAsync(coachId) ?? throw new NotFoundException("Interviewer not found");

                // Create payment and payout transactions with status 'Created'
                Domain.Entities.InterviewBookingTransaction t = new()
                {
                    OrderCode = RandomGenerator.GenerateOrderCode(),
                    UserId = candidateId,
                    Amount = coach.CurrentAmount ?? 0,
                    Status = coach.CurrentAmount == 0 ? TransactionStatus.Paid : TransactionStatus.Created,
                    Type = TransactionType.Payment,
                    CoachAvailabilityId = coachAvailabilityId,
                };

                Domain.Entities.InterviewBookingTransaction t2 = new()
                {
                    OrderCode = RandomGenerator.GenerateOrderCode(),
                    UserId = coachId,
                    Amount = coach.CurrentAmount ?? 0,
                    Status = coach.CurrentAmount == 0 ? TransactionStatus.Paid : TransactionStatus.Created,
                    Type = TransactionType.Payout,
                    CoachAvailabilityId = coachAvailabilityId,
                };

                await _transactionRepository.AddAsync(t);
                await _transactionRepository.AddAsync(t2);
                await _transactionRepository.SaveChangesAsync();
                await _coachAvailabilitiesRepository.SaveChangesAsync();

                if (t.Amount == 0) return null;

                // Create PAYOS payment order and get checkout URL
                string? checkoutUrl = await _paymentService.CreatePaymentOrderAsync(
                    t.OrderCode,
                    t.Amount,
                    $"Book interview",
                    returnUrl,
                    4
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

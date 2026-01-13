using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.InterviewBooking
{
    internal class CreateBookingCheckoutUrl : ICreateBookingCheckoutUrl
    {
        private readonly IPaymentService _paymentService;
        private readonly ICoachProfileRepository _coachProfileRepository;
        private readonly ITransactionRepository _transactionRepository;

        public CreateBookingCheckoutUrl(IPaymentService paymentService, ICoachProfileRepository coachProfileRepository, ITransactionRepository transactionRepository) 
        {
            _paymentService = paymentService;
            _coachProfileRepository = coachProfileRepository;
            _transactionRepository = transactionRepository;
        }

        public async Task<string> ExecuteAsync(Guid candidateId, Guid coachId, Guid coachAvailabilityId, string returnUrl)
        {
            var coach = await _coachProfileRepository.GetProfileByIdAsync(coachId);

            if (coach == null)
            {
                throw new Exception("Interviewer not found");
            }

            Domain.Entities.InterviewBookingTransaction t = new()
            {
                UserId = candidateId,
                Amount = coach.CurrentAmount ?? 0,
                Status = Domain.Entities.Constants.TransactionStatus.Created,
                Type = Domain.Entities.Constants.TransactionType.Payment,
                CoachAvailabilityId = coachAvailabilityId,
            };

            Domain.Entities.InterviewBookingTransaction t2 = new()
            {
                UserId = coachId,
                Amount = coach.CurrentAmount ?? 0,
                Status = Domain.Entities.Constants.TransactionStatus.Created,
                Type = Domain.Entities.Constants.TransactionType.Payout,
                CoachAvailabilityId = coachAvailabilityId,
            };

            await _transactionRepository.AddAsync(t);
            await _transactionRepository.AddAsync(t2);
            await _transactionRepository.SaveChangesAsync();

            string checkoutUrl = await _paymentService.CreatePaymentOrderAsync(
                null,
                t.Amount,
                $"Book interview",
                returnUrl
            );

            return checkoutUrl;
        }
    }
}

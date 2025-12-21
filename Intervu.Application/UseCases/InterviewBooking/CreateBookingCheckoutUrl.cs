using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.InterviewBooking
{
    internal class CreateBookingCheckoutUrl : ICreateBookingCheckoutUrl
    {
        private readonly IPaymentService _paymentService;
        private readonly IInterviewerProfileRepository _interviewerProfileRepository;
        private readonly ITransactionRepository _transactionRepository;

        public CreateBookingCheckoutUrl(IPaymentService paymentService, IInterviewerProfileRepository interviewerProfileRepository, ITransactionRepository transactionRepository) 
        {
            _paymentService = paymentService;
            _interviewerProfileRepository = interviewerProfileRepository;
            _transactionRepository = transactionRepository;
        }

        public async Task<string> ExecuteAsync(Guid intervieweeId, Guid interviewerId, Guid interviewerAvailabilityId, string returnUrl)
        {
            var interviewer = await _interviewerProfileRepository.GetProfileByIdAsync(interviewerId);

            if (interviewer == null)
            {
                throw new Exception("Interviewer not found");
            }

            Domain.Entities.InterviewBookingTransaction t = new()
            {
                UserId = intervieweeId,
                Amount = interviewer.CurrentAmount ?? 0,
                Status = Domain.Entities.Constants.TransactionStatus.Created,
                Type = Domain.Entities.Constants.TransactionType.Payment,
                InterviewerAvailabilityId = interviewerAvailabilityId,
            };

            Domain.Entities.InterviewBookingTransaction t2 = new()
            {
                UserId = interviewerId,
                Amount = interviewer.CurrentAmount ?? 0,
                Status = Domain.Entities.Constants.TransactionStatus.Created,
                Type = Domain.Entities.Constants.TransactionType.Payout,
                InterviewerAvailabilityId = interviewerAvailabilityId,
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

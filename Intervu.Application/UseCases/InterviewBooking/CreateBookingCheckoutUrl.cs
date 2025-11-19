using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.Repositories;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;

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

        public async Task<string> ExecuteAsync(int intervieweeId, int interviewerId, int interviewerAvailabilityId)
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
                InterviewerAvailabilityId = interviewerAvailabilityId,
            };

            await _transactionRepository.AddAsync(t);
            await _transactionRepository.SaveChangesAsync();

            string checkoutUrl = await _paymentService.CreatePaymentOrderAsync(
                t.Id,
                t.Amount,
                $"Book interview"
            );

            return checkoutUrl;
        }
    }
}

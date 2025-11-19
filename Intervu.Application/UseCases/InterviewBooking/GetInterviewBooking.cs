using Intervu.Application.Interfaces.Repositories;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;
using Intervu.Domain.Entities;

namespace Intervu.Application.UseCases.InterviewBooking
{
    public class GetInterviewBooking : IGetInterviewBooking
    {
        private readonly ITransactionRepository _transactionRepository;

        public GetInterviewBooking(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        public async Task<InterviewBookingTransaction?> ExecuteAsync(int interviewBookingId)
        {
            return await _transactionRepository.GetByIdAsync(interviewBookingId);   
        }
    }
}

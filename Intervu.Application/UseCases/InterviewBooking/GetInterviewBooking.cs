using Intervu.Application.Interfaces.UseCases.InterviewBooking;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.InterviewBooking
{
    public class GetInterviewBooking : IGetInterviewBooking
    {
        private readonly ITransactionRepository _transactionRepository;

        public GetInterviewBooking(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        public async Task<InterviewBookingTransaction?> GetById(Guid interviewBookingId)
        {
            return await _transactionRepository.GetByIdAsync(interviewBookingId);   
        }

        public async Task<InterviewBookingTransaction?> GetByOrderCode(int orderCode)
        {
            return await _transactionRepository.GetByOrderCode(orderCode);
        }
    }
}

using Intervu.Application.Interfaces.UseCases.InterviewBooking;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.InterviewBooking
{
    public class UpdateBookingStatus : IUpdateBookingStatus
    {
        private readonly ITransactionRepository _transactionRepository;

        public UpdateBookingStatus(ITransactionRepository transactionRepository) 
        {
            _transactionRepository = transactionRepository;
        }

        public async Task<InterviewBookingTransaction> ExecuteAsync(Guid bookingId, TransactionStatus transactionStatus)
        {
            var t = await _transactionRepository.GetByIdAsync(bookingId) ?? throw new Exception("Booking transaction not found");

            t.Status = transactionStatus;
            _transactionRepository.UpdateAsync(t);
            await _transactionRepository.SaveChangesAsync();

            return t;
        }
    }
}

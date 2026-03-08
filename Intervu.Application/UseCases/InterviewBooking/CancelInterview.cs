using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;
using Intervu.Application.Utils;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Abstractions.Policies.Interfaces;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.InterviewBooking
{
    internal class CancelInterview : ICancelInterview
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRefundPolicy _refundPolicy;

        public CancelInterview(IUnitOfWork unitOfWork, IRefundPolicy refundPolicy)
        {
            _unitOfWork = unitOfWork;
            _refundPolicy = refundPolicy;
        }

        public async Task<int> ExecuteAsync(Guid interviewRoomId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var interviewRoomRepo = _unitOfWork.GetRepository<IInterviewRoomRepository>();
                var transactionRepo = _unitOfWork.GetRepository<ITransactionRepository>();
                var availabilityRepo = _unitOfWork.GetRepository<ICoachAvailabilitiesRepository>();

                Domain.Entities.InterviewRoom room = await interviewRoomRepo.GetByIdAsync(interviewRoomId)
                    ?? throw new NotFoundException("Interview room not found");
                CoachAvailability availability = await availabilityRepo.GetByIdAsync(room.CurrentAvailabilityId)
                    ?? throw new NotFoundException("Coach availability not found for interview room");

                if (room.CandidateId == null)
                    throw new NotFoundException("Candidate not found for interview room");

                // Cancel the payout transaction (coach-side)
                InterviewBookingTransaction? payout = await transactionRepo.GetByAvailabilityId(room.CurrentAvailabilityId, TransactionType.Payout) ?? throw new NotFoundException("Payout transaction not found");
                payout.Status = TransactionStatus.Cancel;

                // Create a refund transaction (candidate-side) using the original payment amount
                InterviewBookingTransaction? payment = await transactionRepo.GetByAvailabilityId(room.CurrentAvailabilityId, TransactionType.Payment) ?? throw new NotFoundException("Payment transaction not found");
                int refundAmount = _refundPolicy.CalculateRefundAmount(payment.Amount, availability.StartTime, DateTime.UtcNow);

                await transactionRepo.AddAsync(new InterviewBookingTransaction
                {
                    OrderCode = RandomGenerator.GenerateOrderCode(),
                    UserId = room.CandidateId.Value,
                    CoachAvailabilityId = room.CurrentAvailabilityId,
                    Amount = refundAmount,
                    Type = TransactionType.Refund,
                    Status = TransactionStatus.Created
                });

                room.Status = InterviewRoomStatus.Cancelled;
                interviewRoomRepo.UpdateAsync(room);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return refundAmount;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}

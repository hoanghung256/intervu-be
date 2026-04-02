using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;
using Intervu.Application.Interfaces.UseCases.Notification;
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
        private readonly IBackgroundService _jobService;

        public CancelInterview(IUnitOfWork unitOfWork, IRefundPolicy refundPolicy, IBackgroundService jobService)
        {
            _unitOfWork = unitOfWork;
            _refundPolicy = refundPolicy;
            _jobService = jobService;
        }

        public async Task<int> ExecuteAsync(Guid interviewRoomId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var interviewRoomRepo = _unitOfWork.GetRepository<IInterviewRoomRepository>();
                var transactionRepo = _unitOfWork.GetRepository<ITransactionRepository>();
                var availabilityRepo = _unitOfWork.GetRepository<ICoachAvailabilitiesRepository>();
                var bookingRepo = _unitOfWork.GetRepository<IBookingRequestRepository>();

                Domain.Entities.InterviewRoom room = await interviewRoomRepo.GetByIdAsync(interviewRoomId)
                    ?? throw new NotFoundException("Interview room not found");

                if (room.CurrentAvailabilityId == null)
                    throw new NotFoundException("No coach availability linked to this interview room");

                CoachAvailability availability = await availabilityRepo.GetByIdAsync(room.CurrentAvailabilityId.Value)
                    ?? throw new NotFoundException("Coach availability not found for interview room");

                if (room.CandidateId == null)
                    throw new NotFoundException("Candidate not found for interview room");

                // Cancel the payout transaction (coach-side)
                InterviewBookingTransaction? payout = await transactionRepo.GetByAvailabilityId(room.CurrentAvailabilityId.Value, TransactionType.Payout) ?? throw new NotFoundException("Payout transaction not found");
                payout.Status = TransactionStatus.Cancel;

                // Create a refund transaction (candidate-side) using the original payment amount
                InterviewBookingTransaction? payment = await transactionRepo.GetByAvailabilityId(room.CurrentAvailabilityId.Value, TransactionType.Payment) ?? throw new NotFoundException("Payment transaction not found");
                int refundAmount = _refundPolicy.CalculateRefundAmount(payment.Amount, availability.StartTime, DateTime.UtcNow);

                await transactionRepo.AddAsync(new InterviewBookingTransaction
                {
                    OrderCode = RandomGenerator.GenerateOrderCode(),
                    UserId = room.CandidateId.Value,
                    CoachAvailabilityId = room.CurrentAvailabilityId.Value,
                    Amount = refundAmount,
                    Type = TransactionType.Refund,
                    Status = TransactionStatus.Created
                });

                room.Status = InterviewRoomStatus.Cancelled;
                interviewRoomRepo.UpdateAsync(room);

                // If this room belongs to a BookingRequest, also cancel the parent request
                if (room.BookingRequestId.HasValue)
                {
                    var bookingRequest = await bookingRepo.GetByIdAsync(room.BookingRequestId.Value);
                    if (bookingRequest != null)
                    {
                        bookingRequest.Status = BookingRequestStatus.Cancelled;
                        bookingRepo.UpdateAsync(bookingRequest);
                    }
                }

                var candidateId = room.CandidateId.Value;
                _jobService.Enqueue<INotificationUseCase>(uc => uc.CreateAsync(
                    candidateId,
                    NotificationType.SystemAnnouncement,
                    "Interview Cancelled",
                    $"Your interview has been cancelled and a refund of {refundAmount:N0} resources has been processed.",
                    "/interview",
                    null
                ));

                if (room.CoachId.HasValue)
                {
                    var coachId = room.CoachId.Value;
                    _jobService.Enqueue<INotificationUseCase>(uc => uc.CreateAsync(
                        coachId,
                        NotificationType.BookingRejected,
                        "Interview Cancelled",
                        "An upcoming interview with a candidate has been cancelled.",
                        "/interview",
                        null
                    ));
                }

                // TODO: Send email notification to candidate about cancellation and refund

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

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
                var bookingRepo = _unitOfWork.GetRepository<IBookingRequestRepository>();
                var availabilityRepo = _unitOfWork.GetRepository<ICoachAvailabilitiesRepository>();

                Domain.Entities.InterviewRoom room = await interviewRoomRepo.GetByIdAsync(interviewRoomId)
                    ?? throw new NotFoundException("Interview room not found");

                if (!room.IsAvailableForCancel())
                    throw new BadRequestException("This interview can no longer be cancelled (scheduled time has passed or it is not in a scheduled state).");

                if (room.BookingRequestId == null)
                    throw new NotFoundException("No booking request linked to this interview room");

                if (room.CandidateId == null)
                    throw new NotFoundException("Candidate not found for interview room");

                // Find transactions via BookingRequestId
                InterviewBookingTransaction payout = await transactionRepo.GetByBookingRequestId(room.BookingRequestId.Value, TransactionType.Payout)
                    ?? throw new NotFoundException("Payout transaction not found");
                payout.Status = TransactionStatus.Cancel;

                InterviewBookingTransaction payment = await transactionRepo.GetByBookingRequestId(room.BookingRequestId.Value, TransactionType.Payment)
                    ?? throw new NotFoundException("Payment transaction not found");

                int refundAmount = _refundPolicy.CalculateRefundAmount(payment.Amount, room.ScheduledTime ?? DateTime.UtcNow, DateTime.UtcNow);

                await transactionRepo.AddAsync(new InterviewBookingTransaction
                {
                    OrderCode = RandomGenerator.GenerateOrderCode(),
                    UserId = room.CandidateId.Value,
                    BookingRequestId = room.BookingRequestId.Value,
                    Amount = refundAmount,
                    Type = TransactionType.Refund,
                    Status = TransactionStatus.Created
                });

                room.Status = InterviewRoomStatus.Cancelled;
                interviewRoomRepo.UpdateAsync(room);

                // Restore availability blocks: load booking request with rounds and their blocks
                var bookingRequest = await bookingRepo.GetByIdWithDetailsAsync(room.BookingRequestId.Value);
                if (bookingRequest != null)
                {
                    bookingRequest.Status = BookingRequestStatus.Cancelled;
                    bookingRepo.UpdateAsync(bookingRequest);

                    // Find the round matching this room and restore its availability blocks
                    var round = bookingRequest.Rounds.FirstOrDefault(r => r.RoundNumber == room.RoundNumber);
                    if (round?.AvailabilityBlocks != null)
                    {
                        foreach (var block in round.AvailabilityBlocks)
                        {
                            block.Status = CoachAvailabilityStatus.Available;
                            block.InterviewRoundId = null;
                            availabilityRepo.UpdateAsync(block);
                        }
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

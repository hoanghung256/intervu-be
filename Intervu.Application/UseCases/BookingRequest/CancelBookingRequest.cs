using AutoMapper;
using Intervu.Application.DTOs.BookingRequest;
using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.UseCases.BookingRequest;
using Intervu.Domain.Abstractions.Policies.Interfaces;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.BookingRequest
{
    internal class CancelBookingRequest : ICancelBookingRequest
    {
        private readonly IBookingRequestRepository _bookingRepo;
        private readonly IInterviewRoomRepository _roomRepo;
        private readonly ITransactionRepository _transactionRepo;
        private readonly ICoachAvailabilitiesRepository _availabilityRepo;
        private readonly IRefundPolicy _refundPolicy;
        private readonly IMapper _mapper;

        public CancelBookingRequest(
            IBookingRequestRepository bookingRepo,
            IInterviewRoomRepository roomRepo,
            ITransactionRepository transactionRepo,
            ICoachAvailabilitiesRepository availabilityRepo,
            IRefundPolicy refundPolicy,
            IMapper mapper)
        {
            _bookingRepo = bookingRepo;
            _roomRepo = roomRepo;
            _transactionRepo = transactionRepo;
            _availabilityRepo = availabilityRepo;
            _refundPolicy = refundPolicy;
            _mapper = mapper;
        }

        public async Task<BookingRequestDto> ExecuteAsync(Guid candidateId, Guid bookingRequestId)
        {
            var bookingRequest = await _bookingRepo.GetByIdWithDetailsAsync(bookingRequestId)
                ?? throw new NotFoundException("Booking request not found");

            // Only the owning candidate can cancel
            if (bookingRequest.CandidateId != candidateId)
                throw new ForbiddenException("You can only cancel your own booking requests");

            // Only Pending, Accepted or Paid requests can be cancelled
            if (bookingRequest.Status != BookingRequestStatus.Pending &&
                bookingRequest.Status != BookingRequestStatus.Accepted &&
                bookingRequest.Status != BookingRequestStatus.Paid)
            {
                throw new BadRequestException(
                    $"Cannot cancel a booking request with status '{bookingRequest.Status}'. " +
                    "Only Pending, Accepted or Paid requests can be cancelled.");
            }

            // Start cancellation
            bookingRequest.Status = BookingRequestStatus.Cancelled;
            bookingRequest.UpdatedAt = DateTime.UtcNow;

            // Load related interview rooms (if any)
            var rooms = await _roomRepo.GetByBookingRequestIdAsync(bookingRequestId);

            foreach (var room in rooms)
            {
                // Skip rows that are already cancelled, but cancel every other room state.
                if (room.Status == InterviewRoomStatus.Cancelled)
                    continue;

                // Only active sessions should trigger payout/refund handling.
                if (room.Status == InterviewRoomStatus.Scheduled || room.Status == InterviewRoomStatus.Ongoing)
                {
                    // If room has an associated availability and transactions, handle refunds similar to CancelInterview
                    if (room.CurrentAvailabilityId.HasValue)
                    {
                        var availability = await _availabilityRepo.GetByIdAsync(room.CurrentAvailabilityId.Value);

                        // Cancel payout transaction (coach-side)
                        var payout = await _transactionRepo.GetByAvailabilityId(room.CurrentAvailabilityId.Value, Domain.Entities.Constants.TransactionType.Payout);
                        if (payout != null)
                        {
                            payout.Status = Domain.Entities.Constants.TransactionStatus.Cancel;
                            _transactionRepo.UpdateAsync(payout);
                        }

                        // Find payment transaction and create refund
                        var payment = await _transactionRepo.GetByAvailabilityId(room.CurrentAvailabilityId.Value, Domain.Entities.Constants.TransactionType.Payment);
                        if (payment != null)
                        {
                            int refundAmount;

                            if (availability != null)
                            {
                                try
                                {
                                    refundAmount = _refundPolicy.CalculateRefundAmount(payment.Amount, availability.StartTime, DateTime.UtcNow);
                                }
                                catch
                                {
                                    refundAmount = payment.Amount;
                                }
                            }
                            else
                            {
                                refundAmount = payment.Amount;
                            }

                            await _transactionRepo.AddAsync(new Domain.Entities.InterviewBookingTransaction
                            {
                                OrderCode = Intervu.Application.Utils.RandomGenerator.GenerateOrderCode(),
                                UserId = room.CandidateId!.Value,
                                CoachAvailabilityId = room.CurrentAvailabilityId.Value,
                                Amount = refundAmount,
                                Type = Domain.Entities.Constants.TransactionType.Refund,
                                Status = Domain.Entities.Constants.TransactionStatus.Created
                            });
                        }
                    }
                }

                room.Status = InterviewRoomStatus.Cancelled;
                _roomRepo.UpdateAsync(room);
            }

            _bookingRepo.UpdateAsync(bookingRequest);
            await _bookingRepo.SaveChangesAsync();

            var result = _mapper.Map<BookingRequestDto>(bookingRequest);
            result.CandidateName = bookingRequest.Candidate?.User?.FullName;
            result.CoachName = bookingRequest.Coach?.User?.FullName;

            return result;
        }
    }
}

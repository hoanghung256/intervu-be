using AutoMapper;
using Intervu.Application.DTOs.BookingRequest;
using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.BookingRequest;
using Intervu.Application.Utils;
using Intervu.Domain.Abstractions.Policies.Interfaces;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.BookingRequest
{
    internal class CancelInterviewRound : ICancelInterviewRound
    {
        private readonly IBookingRequestRepository _bookingRepo;
        private readonly IInterviewRoomRepository _roomRepo;
        private readonly ITransactionRepository _transactionRepo;
        private readonly ICoachAvailabilitiesRepository _availabilityRepo;
        private readonly IRefundPolicy _refundPolicy;
        private readonly IPaymentService _paymentService;
        private readonly IBankFieldProtector _bankFieldProtector;
        private readonly IMapper _mapper;

        public CancelInterviewRound(
            IBookingRequestRepository bookingRepo,
            IInterviewRoomRepository roomRepo,
            ITransactionRepository transactionRepo,
            ICoachAvailabilitiesRepository availabilityRepo,
            IRefundPolicy refundPolicy,
            IPaymentService paymentService,
            IBankFieldProtector bankFieldProtector,
            IMapper mapper)
        {
            _bookingRepo = bookingRepo;
            _roomRepo = roomRepo;
            _transactionRepo = transactionRepo;
            _availabilityRepo = availabilityRepo;
            _refundPolicy = refundPolicy;
            _paymentService = paymentService;
            _bankFieldProtector = bankFieldProtector;
            _mapper = mapper;
        }

        public async Task<BookingRequestDto> ExecuteAsync(Guid candidateId, Guid bookingRequestId, Guid roundId)
        {
            var bookingRequest = await _bookingRepo.GetByIdWithDetailsAsync(bookingRequestId)
                ?? throw new NotFoundException("Booking request not found");

            if (bookingRequest.CandidateId != candidateId)
                throw new ForbiddenException("You can only cancel rounds from your own booking requests");

            if (bookingRequest.Status != BookingRequestStatus.Accepted)
                throw new BadRequestException(
                    $"Cannot cancel a round on a booking request with status '{bookingRequest.Status}'. " +
                    "Only Accepted bookings can have individual rounds cancelled.");

            var round = bookingRequest.Rounds.FirstOrDefault(r => r.Id == roundId)
                ?? throw new NotFoundException("Round not found in this booking request");

            if (round.Status == InterviewRoundStatus.Cancelled)
                throw new BadRequestException("This round is already cancelled");

            // Validate the interview room is still cancellable
            if (round.InterviewRoom != null &&
                round.InterviewRoom.Status != InterviewRoomStatus.Scheduled)
            {
                throw new BadRequestException(
                    $"Cannot cancel round {round.RoundNumber}: the interview session is already {round.InterviewRoom.Status}");
            }

            // Mark round as cancelled
            round.Status = InterviewRoundStatus.Cancelled;
            round.UpdatedAt = DateTime.UtcNow;

            // Cancel the corresponding interview room
            if (round.InterviewRoom != null)
            {
                round.InterviewRoom.Status = InterviewRoomStatus.Cancelled;
                _roomRepo.UpdateAsync(round.InterviewRoom);
            }

            // Free availability blocks for this round
            foreach (var block in round.AvailabilityBlocks ?? [])
            {
                block.Status = CoachAvailabilityStatus.Available;
                block.InterviewRoundId = null;
                _availabilityRepo.UpdateAsync(block);
            }

            // Create partial refund using RefundPolicy
            var payment = await _transactionRepo.GetByBookingRequestId(bookingRequestId, TransactionType.Payment);
            if (payment != null && round.Price > 0)
            {
                var refundAmount = _refundPolicy.CalculateRefundAmount(round.Price, round.StartTime, DateTime.UtcNow);
                if (refundAmount > 0)
                {
                    var refundTx = new InterviewBookingTransaction
                    {
                        OrderCode = RandomGenerator.GenerateOrderCode(),
                        UserId = candidateId,
                        BookingRequestId = bookingRequestId,
                        Amount = refundAmount,
                        Type = TransactionType.Refund,
                        Status = TransactionStatus.Created
                    };

                    await _transactionRepo.AddAsync(refundTx);

                    var bankBin = bookingRequest.Candidate?.BankBinNumber;
                    var encryptedAccountNumber = bookingRequest.Candidate?.BankAccountNumber;
                    if (string.IsNullOrWhiteSpace(bankBin) || string.IsNullOrWhiteSpace(encryptedAccountNumber))
                        throw new BadRequestException("Candidate bank information is missing");

                    string accountNumber;
                    try
                    {
                        accountNumber = _bankFieldProtector.Decrypt(encryptedAccountNumber);
                    }
                    catch
                    {
                        // Backward compatibility: fallback for legacy plain-text account values.
                        accountNumber = encryptedAccountNumber;
                    }

                    var isRefundSent = await _paymentService.CreateSpendOrderAsync(
                        refundAmount,
                        "REFUND",
                        bankBin,
                        accountNumber);

                    if (isRefundSent)
                    {
                        refundTx.Status = TransactionStatus.Paid;
                        _transactionRepo.UpdateAsync(refundTx);
                    }
                }
            }

            // If this was the last active round, cancel the entire booking
            var hasActiveRounds = bookingRequest.Rounds.Any(r => r.Id != roundId && r.Status == InterviewRoundStatus.Active);
            if (!hasActiveRounds)
            {
                bookingRequest.Status = BookingRequestStatus.Cancelled;
                bookingRequest.UpdatedAt = DateTime.UtcNow;

                // Cancel the payout — coach will not be paid for any remaining sessions
                var payout = await _transactionRepo.GetByBookingRequestId(bookingRequestId, TransactionType.Payout);
                if (payout != null)
                {
                    payout.Status = TransactionStatus.Cancel;
                    _transactionRepo.UpdateAsync(payout);
                }
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

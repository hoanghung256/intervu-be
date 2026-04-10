using AutoMapper;
using Intervu.Application.DTOs.BookingRequest;
using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.ExternalServices.Email;
using Intervu.Application.Interfaces.UseCases.BookingRequest;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;
using Intervu.Application.UseCases.InterviewBooking;
using Intervu.Domain.Abstractions.Policies.Interfaces;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Hosting;

namespace Intervu.Application.UseCases.BookingRequest
{
    internal class CancelBookingRequest : ICancelBookingRequest
    {
        private readonly IBookingRequestRepository _bookingRepo;
        private readonly IInterviewRoomRepository _roomRepo;
        private readonly ITransactionRepository _transactionRepo;
        private readonly ICoachAvailabilitiesRepository _availabilityRepo;
        private readonly IRefundPolicy _refundPolicy;
        private readonly IRefundForCandidate _refundForCandidate;
        private readonly IMapper _mapper;
        private readonly IBackgroundService _backgroundService;
        private readonly IUserRepository _userRepository;

        public CancelBookingRequest(
            IBookingRequestRepository bookingRepo,
            IInterviewRoomRepository roomRepo,
            ITransactionRepository transactionRepo,
            ICoachAvailabilitiesRepository availabilityRepo,
            IRefundPolicy refundPolicy,
            IMapper mapper,
            IBackgroundService backgroundService,
            IRefundForCandidate refundForCandidate,
            IUserRepository userRepository)
        {
            _bookingRepo = bookingRepo;
            _roomRepo = roomRepo;
            _transactionRepo = transactionRepo;
            _availabilityRepo = availabilityRepo;
            _refundPolicy = refundPolicy;
            _refundForCandidate = refundForCandidate;
            _mapper = mapper;
            _backgroundService = backgroundService;
            _userRepository = userRepository;
        }

        public async Task<BookingRequestDto> ExecuteAsync(Guid candidateId, Guid bookingRequestId)
        {
            var bookingRequest = await _bookingRepo.GetByIdWithDetailsAsync(bookingRequestId)
                ?? throw new NotFoundException("Booking request not found");

            // Only the owning candidate can cancel
            if (bookingRequest.CandidateId != candidateId)
                throw new ForbiddenException("You can only cancel your own booking requests");

            // Only Pending, PendingForApprovalAfterPayment or Accepted requests can be cancelled
            if (bookingRequest.Status != BookingRequestStatus.Pending &&
                bookingRequest.Status != BookingRequestStatus.PendingForApprovalAfterPayment &&
                bookingRequest.Status != BookingRequestStatus.Accepted)
            {
                throw new BadRequestException(
                    $"Cannot cancel a booking request with status '{bookingRequest.Status}'. " +
                    "Only Pending, PendingForApprovalAfterPayment or Accepted requests can be cancelled.");
            }

            // Start cancellation
            bookingRequest.Status = BookingRequestStatus.Cancelled;
            bookingRequest.UpdatedAt = DateTime.UtcNow;

            // Handle refunds via BookingRequest transactions
            var payout = await _transactionRepo.GetByBookingRequestId(bookingRequestId, TransactionType.Payout);
            if (payout != null)
            {
                payout.Status = TransactionStatus.Cancel;
                _transactionRepo.UpdateAsync(payout);
            }

            int refundAmount = 0;
            var payment = await _transactionRepo.GetByBookingRequestId(bookingRequestId, TransactionType.Payment);
            if (payment != null)
            {
                var firstRound = bookingRequest.Rounds.OrderBy(r => r.RoundNumber).FirstOrDefault();
                var scheduledTime = firstRound?.StartTime ?? DateTime.UtcNow;

                try
                {
                    refundAmount = _refundPolicy.CalculateRefundAmount(payment.Amount, scheduledTime, DateTime.UtcNow);
                }
                catch
                {
                    refundAmount = payment.Amount;
                }

                // Refund for Candidate
                _backgroundService.Enqueue<IPaymentService>(
                    uc => uc.CreateSpendOrderAsync(
                        refundAmount, 
                        "REFUND", 
                        bookingRequest.Candidate.BankBinNumber,
                        bookingRequest.Candidate.BankAccountNumber
                )); 
                //await _transactionRepo.AddAsync(new Domain.Entities.InterviewBookingTransaction
                //{
                //    OrderCode = Intervu.Application.Utils.RandomGenerator.GenerateOrderCode(),
                //    UserId = bookingRequest.CandidateId,
                //    BookingRequestId = bookingRequestId,
                //    Amount = refundAmount,
                //    Type = TransactionType.Refund,
                //    Status = TransactionStatus.Created
                //});

            }

            // Cancel all related interview rooms
            var rooms = await _roomRepo.GetByBookingRequestIdAsync(bookingRequestId);
            foreach (var room in rooms)
            {
                if (room.Status == InterviewRoomStatus.Cancelled)
                    continue;

                room.Status = InterviewRoomStatus.Cancelled;
                _roomRepo.UpdateAsync(room);
            }

            // Restore availability blocks for all rounds back to Available
            foreach (var round in bookingRequest.Rounds)
            {
                if (round.AvailabilityBlocks == null) continue;
                foreach (var block in round.AvailabilityBlocks)
                {
                    block.Status = CoachAvailabilityStatus.Available;
                    block.InterviewRoundId = null;
                    _availabilityRepo.UpdateAsync(block);
                }
            }

            _bookingRepo.UpdateAsync(bookingRequest);
            await _bookingRepo.SaveChangesAsync();

            try
            {
                var candidate = await _userRepository.GetByIdAsync(bookingRequest.CandidateId);
                var coach = await _userRepository.GetByIdAsync(bookingRequest.CoachId);

                if (candidate != null)
                {
                    var candidatePlaceholders = new Dictionary<string, string>
                    {
                        ["RecipientName"] = candidate.FullName,
                        ["OtherPartyName"] = coach?.FullName ?? "Coach",
                        ["RefundAmount"] = refundAmount.ToString("N0")
                    };

                    _backgroundService.Enqueue<IEmailService>(svc => svc.SendEmailWithTemplateAsync(
                        candidate.Email,
                        "BookingRequestCancelled",
                        candidatePlaceholders));
                }

                if (coach != null)
                {
                    var coachPlaceholders = new Dictionary<string, string>
                    {
                        ["RecipientName"] = coach.FullName,
                        ["OtherPartyName"] = candidate?.FullName ?? "Candidate",
                        ["RefundAmount"] = refundAmount.ToString("N0")
                    };

                    _backgroundService.Enqueue<IEmailService>(svc => svc.SendEmailWithTemplateAsync(
                        coach.Email,
                        "BookingRequestCancelled",
                        coachPlaceholders));
                }
            }
            catch
            {
                // Do not fail booking cancellation flow if email enqueue fails.
            }

            var result = _mapper.Map<BookingRequestDto>(bookingRequest);
            result.CandidateName = bookingRequest.Candidate?.User?.FullName;
            result.CoachName = bookingRequest.Coach?.User?.FullName;

            return result;
        }
    }
}

using AutoMapper;
using Intervu.Application.DTOs.BookingRequest;
using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.ExternalServices.Email;
using Intervu.Application.Interfaces.UseCases.BookingRequest;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Application.Interfaces.UseCases.Notification;
using Intervu.Application.Utils;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.BookingRequest
{
    internal class RespondToBookingRequest : IRespondToBookingRequest
    {
        private readonly IBookingRequestRepository _bookingRepo;
        private readonly ITransactionRepository _transactionRepo;
        private readonly ICoachAvailabilitiesRepository _availabilityRepo;
        private readonly ICreateEvaluationResultsUseCase _createEvaluationResultsUseCase;
        private readonly IBackgroundService _backgroundService;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;

        public RespondToBookingRequest(
            IBookingRequestRepository bookingRepo,
            IUserRepository userRepository,
            ITransactionRepository transactionRepo,
            ICoachAvailabilitiesRepository availabilityRepo,
            ICreateEvaluationResultsUseCase createEvaluationResultsUseCase,
            IBackgroundService backgroundService,
            IMapper mapper)
        {
            _bookingRepo = bookingRepo;
            _transactionRepo = transactionRepo;
            _availabilityRepo = availabilityRepo;
            _createEvaluationResultsUseCase = createEvaluationResultsUseCase;
            _mapper = mapper;
            _backgroundService = backgroundService;
            _userRepository = userRepository;
        }

        public async Task<BookingRequestDto> ExecuteAsync(Guid coachId, Guid bookingRequestId, RespondToBookingRequestDto dto)
        {
            var bookingRequest = await _bookingRepo.GetByIdWithDetailsAsync(bookingRequestId)
                ?? throw new NotFoundException("Booking request not found");

            if (bookingRequest.CoachId != coachId)
                throw new ForbiddenException("You can only respond to booking requests addressed to you");

            // Only PendingForApprovalAfterPayment requests can be responded to — candidate has paid, coach must approve or reject
            if (bookingRequest.Status != BookingRequestStatus.PendingForApprovalAfterPayment)
                throw new BadRequestException($"Cannot respond to a booking request with status '{bookingRequest.Status}'");

            // Check if the coach response window has expired
            if (bookingRequest.ExpiresAt.HasValue && bookingRequest.ExpiresAt.Value <= DateTime.UtcNow)
            {
                bookingRequest.Status = BookingRequestStatus.Expired;
                bookingRequest.UpdatedAt = DateTime.UtcNow;
                _bookingRepo.UpdateAsync(bookingRequest);
                await _bookingRepo.SaveChangesAsync();
                throw new BadRequestException("This booking request has expired");
            }

            if (dto.IsApproved)
            {
                await HandleApprovalAsync(bookingRequest);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(dto.RejectionReason))
                    throw new BadRequestException("Rejection reason is required when rejecting a booking request");

                await HandleRejectionAsync(bookingRequest, dto.RejectionReason);
            }

            bookingRequest.RespondedAt = DateTime.UtcNow;
            bookingRequest.UpdatedAt = DateTime.UtcNow;

            _bookingRepo.UpdateAsync(bookingRequest);
            await _bookingRepo.SaveChangesAsync();

            if (!dto.IsApproved)
            {
                try
                {
                    var candidate = await _userRepository.GetByIdAsync(bookingRequest.CandidateId);
                    var coach = await _userRepository.GetByIdAsync(bookingRequest.CoachId);

                    if (candidate != null)
                    {
                        var placeholders = new Dictionary<string, string>
                        {
                            ["CandidateName"] = candidate.FullName,
                            ["CoachName"] = coach?.FullName ?? "Coach",
                            ["RejectionReason"] = bookingRequest.RejectionReason ?? "The coach declined this request."
                        };

                        _backgroundService.Enqueue<IEmailService>(svc => svc.SendEmailWithTemplateAsync(
                            candidate.Email,
                            "BookingRequestRejected",
                            placeholders));
                    }
                }
                catch
                {
                    // Do not fail booking request response if email enqueue fails.
                }
            }

            var result = _mapper.Map<BookingRequestDto>(bookingRequest);
            result.CandidateName = bookingRequest.Candidate?.User?.FullName;
            result.CoachName = bookingRequest.Coach?.User?.FullName;

            return result;
        }

        private async Task HandleApprovalAsync(Domain.Entities.BookingRequest bookingRequest)
        {
            bookingRequest.Status = BookingRequestStatus.Accepted;

            var paymentTx = await _transactionRepo.GetByBookingRequestId(bookingRequest.Id, TransactionType.Payment);

            foreach (var round in bookingRequest.Rounds.OrderBy(r => r.RoundNumber))
            {
                var roundDuration = round.CoachInterviewService?.DurationMinutes ?? 60;
                var firstBlockId = round.AvailabilityBlocks?.OrderBy(b => b.StartTime).FirstOrDefault()?.Id;

                var room = new Domain.Entities.InterviewRoom
                {
                    CandidateId = bookingRequest.CandidateId,
                    CoachId = bookingRequest.CoachId,
                    ScheduledTime = round.StartTime,
                    DurationMinutes = roundDuration,
                    CurrentAvailabilityId = firstBlockId,
                    Status = InterviewRoomStatus.Scheduled,
                    TransactionId = paymentTx?.Id,
                    BookingRequestId = bookingRequest.Id,
                    CoachInterviewServiceId = round.CoachInterviewServiceId,
                    AimLevel = bookingRequest.AimLevel,
                    RoundNumber = round.RoundNumber,
                    EvaluationResults = await _createEvaluationResultsUseCase.ExecuteAsync(round.CoachInterviewServiceId),
                    IsEvaluationCompleted = false
                };

                _backgroundService.Enqueue<ICreateInterviewRoom>(uc => uc.ExecuteAsync(room));
            }

            _backgroundService.Enqueue<INotificationUseCase>(
                uc => uc.CreateAsync(
                    bookingRequest.CandidateId,
                    NotificationType.BookingAccepted,
                    "Booking confirmed",
                    "Your interview booking has been accepted by the coach.",
                    "/interview?tab=upcoming",
                    null));
        }

        private async Task HandleRejectionAsync(Domain.Entities.BookingRequest bookingRequest, string rejectionReason)
        {
            bookingRequest.Status = BookingRequestStatus.Rejected;
            bookingRequest.RejectionReason = rejectionReason;

            // Cancel the payout — coach will not receive payment
            var payout = await _transactionRepo.GetByBookingRequestId(bookingRequest.Id, TransactionType.Payout);
            if (payout != null)
            {
                payout.Status = TransactionStatus.Cancel;
                _transactionRepo.UpdateAsync(payout);
            }

            // Issue 100% refund to candidate
            var payment = await _transactionRepo.GetByBookingRequestId(bookingRequest.Id, TransactionType.Payment);
            if (payment != null)
            {
                await _transactionRepo.AddAsync(new InterviewBookingTransaction
                {
                    OrderCode = RandomGenerator.GenerateOrderCode(),
                    UserId = bookingRequest.CandidateId,
                    BookingRequestId = bookingRequest.Id,
                    Amount = payment.Amount,
                    Type = TransactionType.Refund,
                    Status = TransactionStatus.Created
                });
            }

            // Free up availability blocks so the coach's slots become available again
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

            _backgroundService.Enqueue<INotificationUseCase>(
                uc => uc.CreateAsync(
                    bookingRequest.CandidateId,
                    NotificationType.BookingRejected,
                    "Booking rejected",
                    "Your interview booking was rejected by the coach. A full refund will be processed.",
                    "/booking?tab=history",
                    null));
        }

        
    }
}

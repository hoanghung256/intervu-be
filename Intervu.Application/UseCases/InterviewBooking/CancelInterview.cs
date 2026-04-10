using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.ExternalServices.Email;
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
        private readonly IPaymentService _paymentService;
        private readonly IUserRepository _userRepository;

        public CancelInterview(
            IUnitOfWork unitOfWork,
            IRefundPolicy refundPolicy,
            IBackgroundService jobService,
            IPaymentService paymentService,
            IUserRepository userRepository)
        {
            _unitOfWork = unitOfWork;
            _refundPolicy = refundPolicy;
            _jobService = jobService;
            _paymentService = paymentService;
            _userRepository = userRepository;
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
                if (bookingRequest == null) throw new NotFoundException("Booking request not found for interview room");

                bookingRequest.Status = BookingRequestStatus.Cancelled;
                bookingRepo.UpdateAsync(bookingRequest);

                // Refund candidate
                //_jobService.Enqueue<IPaymentService>(
                //    uc => uc.CreateSpendOrderAsync(
                //        refundAmount, 
                //        "REFUND", 
                //        bookingRequest.Candidate.BankBinNumber,
                //        bookingRequest.Candidate.BankAccountNumber
                //    )
                //);
                await _paymentService.CreateSpendOrderAsync(
                        refundAmount, 
                        "REFUND", 
                        bookingRequest.Candidate.BankBinNumber,
                        bookingRequest.Candidate.BankAccountNumber
                    );

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

                try
                {
                    var candidate = await _userRepository.GetByIdAsync(candidateId);
                    var coach = room.CoachId.HasValue
                        ? await _userRepository.GetByIdAsync(room.CoachId.Value)
                        : null;

                    var interviewDate = bookingRequest?.RequestedStartTime?.ToString("dd MMM yyyy HH:mm")
                        ?? room.ScheduledTime?.ToString("dd MMM yyyy HH:mm")
                        ?? "TBD";

                    if (candidate != null)
                    {
                        var candidatePlaceholders = new Dictionary<string, string>
                        {
                            ["RecipientName"] = candidate.FullName,
                            ["OtherPartyName"] = coach?.FullName ?? "Coach",
                            ["InterviewDate"] = interviewDate,
                            ["RefundAmount"] = refundAmount.ToString("N0"),
                            ["RefundNote"] = $"A refund of {refundAmount:N0} resources has been processed to your account."
                        };

                        _jobService.Enqueue<IEmailService>(svc => svc.SendEmailWithTemplateAsync(
                            candidate.Email,
                            "InterviewCancellation",
                            candidatePlaceholders));
                    }

                    if (coach != null)
                    {
                        var coachPlaceholders = new Dictionary<string, string>
                        {
                            ["RecipientName"] = coach.FullName,
                            ["OtherPartyName"] = candidate?.FullName ?? "Candidate",
                            ["InterviewDate"] = interviewDate,
                            ["RefundAmount"] = "0",
                            ["RefundNote"] = "This cancellation was initiated by the candidate. No payout will be processed for this session."
                        };

                        _jobService.Enqueue<IEmailService>(svc => svc.SendEmailWithTemplateAsync(
                            coach.Email,
                            "InterviewCancellation",
                            coachPlaceholders));
                    }
                }
                catch
                {
                    // Do not fail cancellation flow if background email enqueue fails.
                }

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

using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.BookingRequest;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Application.Utils;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Intervu.Application.UseCases.BookingRequest
{
    internal class PayBookingRequest : IPayBookingRequest
    {
        private readonly ILogger<PayBookingRequest> _logger;
        private readonly IPaymentService _paymentService;
        private readonly IBackgroundService _backgroundService;
        private readonly ICreateEvaluationResultsUseCase _createEvaluationResults;
        private readonly IUnitOfWork _unitOfWork;

        public PayBookingRequest(
            ILogger<PayBookingRequest> logger,
            IPaymentService paymentService,
            IBackgroundService backgroundService,
            ICreateEvaluationResultsUseCase createEvaluationResults,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _paymentService = paymentService;
            _backgroundService = backgroundService;
            _createEvaluationResults = createEvaluationResults;
            _unitOfWork = unitOfWork;
        }

        public async Task<string?> ExecuteAsync(Guid candidateId, Guid bookingRequestId, string returnUrl)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var bookingRepo = _unitOfWork.GetRepository<IBookingRequestRepository>();
                var transactionRepo = _unitOfWork.GetRepository<ITransactionRepository>();

                var bookingRequest = await bookingRepo.GetByIdWithDetailsAsync(bookingRequestId)
                    ?? throw new NotFoundException("Booking request not found");

                // Only the owning candidate can pay
                if (bookingRequest.CandidateId != candidateId)
                    throw new ForbiddenException("You can only pay for your own booking requests");

                // Only Pending requests can be paid
                if (bookingRequest.Status != BookingRequestStatus.Pending)
                    throw new BadRequestException(
                        $"Cannot pay for a booking request with status '{bookingRequest.Status}'. " +
                        "Only Pending requests can be paid.");

                int paymentAmount = bookingRequest.TotalAmount;

                // Create payment transaction (candidate pays)
                InterviewBookingTransaction paymentTx = new()
                {
                    OrderCode = RandomGenerator.GenerateOrderCode(),
                    UserId = candidateId,
                    Amount = paymentAmount,
                    Status = TransactionStatus.Created,
                    Type = TransactionType.Payment,
                    BookingRequestId = bookingRequestId,
                };

                // Create payout transaction (coach receives)
                InterviewBookingTransaction payoutTx = new()
                {
                    OrderCode = RandomGenerator.GenerateOrderCode(),
                    UserId = bookingRequest.CoachId,
                    Amount = paymentAmount,
                    Status = TransactionStatus.Created,
                    Type = TransactionType.Payout,
                    BookingRequestId = bookingRequestId,
                };

                await transactionRepo.AddAsync(paymentTx);
                await transactionRepo.AddAsync(payoutTx);

                string? checkoutUrl = null;
                if (paymentAmount == 0)
                {
                    // Free booking — auto-accept immediately, upgrade blocks and create rooms
                    paymentTx.Status = TransactionStatus.Paid;
                    payoutTx.Status = TransactionStatus.Paid;
                    bookingRequest.Status = BookingRequestStatus.Accepted;
                    bookingRequest.ExpiresAt = DateTime.UtcNow.AddHours(48);
                    bookingRequest.UpdatedAt = DateTime.UtcNow;

                    var availabilityRepo = _unitOfWork.GetRepository<ICoachAvailabilitiesRepository>();
                    foreach (var round in bookingRequest.Rounds)
                    {
                        if (round.AvailabilityBlocks == null) continue;
                        foreach (var block in round.AvailabilityBlocks)
                        {
                            block.Status = CoachAvailabilityStatus.Booked;
                            availabilityRepo.UpdateAsync(block);
                        }
                    }

                    // Create interview rooms for each round and persist them inside the same
                    // transaction so the InterviewRound.InterviewRoomId FK is valid at SaveChanges.
                    var roomRepo = _unitOfWork.GetRepository<IInterviewRoomRepository>();
                    foreach (var round in bookingRequest.Rounds)
                    {
                        var availabilityId = round.AvailabilityBlocks?.FirstOrDefault()?.Id ?? Guid.Empty;
                        var duration = (int)(round.EndTime - round.StartTime).TotalMinutes;

                        var room = new Domain.Entities.InterviewRoom
                        {
                            Id = Guid.NewGuid(),
                            CandidateId = bookingRequest.CandidateId,
                            CoachId = bookingRequest.CoachId,
                            ScheduledTime = round.StartTime,
                            DurationMinutes = duration,
                            CurrentAvailabilityId = availabilityId,
                            Status = InterviewRoomStatus.Scheduled,
                            TransactionId = paymentTx.Id,
                            BookingRequestId = bookingRequest.Id,
                            CoachInterviewServiceId = round.CoachInterviewServiceId,
                            AimLevel = bookingRequest.AimLevel,
                            RoundNumber = round.RoundNumber,
                            EvaluationResults = await _createEvaluationResults.ExecuteAsync(round.CoachInterviewServiceId),
                            IsEvaluationCompleted = false
                        };

                        await roomRepo.AddAsync(room);
                        round.InterviewRoomId = room.Id;
                    }
                }
                else
                {
                    // Paid booking: extend the 5-min reservation hold to cover checkout duration
                    bookingRequest.ExpiresAt = DateTime.UtcNow.AddHours(48);
                    bookingRequest.UpdatedAt = DateTime.UtcNow;

                    string description = bookingRequest.Type switch
                    {
                        BookingRequestType.Direct => "Direct booking",
                        _ => "JD multi-round booking"
                    };

                    checkoutUrl = await _paymentService.CreatePaymentOrderAsync(
                        paymentTx.OrderCode,
                        paymentTx.Amount,
                        description,
                        returnUrl,
                        4 // expire after 4 minutes
                    );
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation(
                    "Payment initiated for BookingRequest {BookingRequestId}, Amount: {Amount}",
                    bookingRequestId, paymentAmount);

                return checkoutUrl;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}

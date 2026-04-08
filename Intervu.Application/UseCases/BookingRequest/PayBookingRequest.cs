using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.BookingRequest;
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
        private readonly IUnitOfWork _unitOfWork;

        public PayBookingRequest(
            ILogger<PayBookingRequest> logger,
            IPaymentService paymentService,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _paymentService = paymentService;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Creates interview rooms from booking request rounds.
        /// Direct and JD bookings both have rounds with pre-booked availability blocks.
        /// </summary>
        private async Task CreateRoomsForBookingRequestAsync(Domain.Entities.BookingRequest bookingRequest, InterviewBookingTransaction paymentTx)
        {
            var roomRepo = _unitOfWork.GetRepository<IInterviewRoomRepository>();
            var coachInterviewServiceRepo = _unitOfWork.GetRepository<ICoachInterviewServiceRepository>();

            foreach (var round in bookingRequest.Rounds.OrderBy(r => r.RoundNumber))
            {
                var roundDuration = round.CoachInterviewService?.DurationMinutes ?? 60;

                // Use the first availability block of this round as the reference
                var firstBlockId = round.AvailabilityBlocks?.OrderBy(b => b.StartTime).FirstOrDefault()?.Id;

                var room = new Domain.Entities.InterviewRoom
                {
                    CandidateId = bookingRequest.CandidateId,
                    CoachId = bookingRequest.CoachId,
                    ScheduledTime = round.StartTime,
                    DurationMinutes = roundDuration,
                    CurrentAvailabilityId = firstBlockId,
                    Status = InterviewRoomStatus.Scheduled,
                    TransactionId = paymentTx.Id,
                    BookingRequestId = bookingRequest.Id,
                    CoachInterviewServiceId = round.CoachInterviewServiceId,
                    AimLevel = bookingRequest.AimLevel,
                    RoundNumber = round.RoundNumber,
                    EvaluationResults = await CreateEvaluationResultsFromInterviewServiceAsync(coachInterviewServiceRepo, round.CoachInterviewServiceId),
                    IsEvaluationCompleted = false
                };

                await roomRepo.AddAsync(room);
                round.InterviewRoom = room;
            }
        }

        private static async Task<List<EvaluationResult>> CreateEvaluationResultsFromInterviewServiceAsync(
            ICoachInterviewServiceRepository coachInterviewServiceRepo,
            Guid? coachInterviewServiceId)
        {
            if (coachInterviewServiceId == null)
                return [];

            var service = await coachInterviewServiceRepo.GetByIdWithDetailsAsync(coachInterviewServiceId.Value);

            if (service == null)
                return [];

            return [.. service.InterviewType.EvaluationStructure.Select(c => new EvaluationResult
            {
                Type = c.Type,
                Question = c.Question,
                Score = 0,
                Answer = ""
            })];
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

                // Only Accepted requests can be paid
                if (bookingRequest.Status != BookingRequestStatus.Accepted)
                    throw new BadRequestException(
                        $"Cannot pay for a booking request with status '{bookingRequest.Status}'. " +
                        "Only Accepted requests can be paid.");

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
                    // Free booking — mark as paid immediately
                    paymentTx.Status = TransactionStatus.Paid;
                    payoutTx.Status = TransactionStatus.Paid;
                    bookingRequest.Status = BookingRequestStatus.Paid;
                    bookingRequest.UpdatedAt = DateTime.UtcNow;

                    // For free bookings, no webhook will fire — create rooms now
                    await CreateRoomsForBookingRequestAsync(bookingRequest, paymentTx);
                }
                else
                {
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

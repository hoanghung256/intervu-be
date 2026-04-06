using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Application.Interfaces.UseCases.Notification;
using Intervu.Application.Utils;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Intervu.Application.UseCases.InterviewBooking
{
    /// <summary>
    /// Subtraction Pattern: CoachAvailability records are never split or deleted.
    /// Free time = Availability windows − Active bookings (computed at query time).
    /// </summary>
    internal class CreateBookingCheckoutUrl : ICreateBookingCheckoutUrl
    {
        private const int AvailabilityBlockMinutes = 30;

        private readonly ILogger<CreateBookingCheckoutUrl> _logger;
        private readonly IPaymentService _paymentService;
        private readonly IBackgroundService _jobService;
        private readonly ICoachInterviewServiceRepository _coachInterviewServiceRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateBookingCheckoutUrl(
            ILogger<CreateBookingCheckoutUrl> logger,
            IPaymentService paymentService,
            IBackgroundService jobService,
            ICoachInterviewServiceRepository coachInterviewServiceRepository,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _paymentService = paymentService;
            _jobService = jobService;
            _coachInterviewServiceRepository = coachInterviewServiceRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<string?> ExecuteAsync(
            Guid candidateId,
            Guid coachId,
            Guid coachAvailabilityId,
            Guid coachInterviewServiceId,
            DateTime startTime,
            string returnUrl)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var availabilityRepo = _unitOfWork.GetRepository<ICoachAvailabilitiesRepository>();
                var transactionRepo = _unitOfWork.GetRepository<ITransactionRepository>();
                var coachRepo = _unitOfWork.GetRepository<ICoachProfileRepository>();
                var serviceRepo = _unitOfWork.GetRepository<ICoachInterviewServiceRepository>();

                // 1. Validate coach exists
                var coach = await coachRepo.GetProfileByIdAsync(coachId)
                    ?? throw new NotFoundException("Interviewer not found");

                // 2. Validate CoachInterviewService exists and belongs to this coach
                var service = await serviceRepo.GetByIdWithDetailsAsync(coachInterviewServiceId)
                    ?? throw new NotFoundException("Coach interview service not found");

                if (service.CoachId != coachId)
                    throw new BadRequestException("The selected service does not belong to the specified coach");

                int paymentAmount = service.Price;
                int duration = service.DurationMinutes;
                var endTime = startTime.AddMinutes(duration);

                // 3. Validate the starting availability slot exists, belongs to coach, and is Available
                var availability = await availabilityRepo.GetByIdForUpdateAsync(coachAvailabilityId)
                    ?? throw new NotFoundException("Coach availability not found");

                if (availability.CoachId != coachId)
                    throw new BadRequestException("Coach availability does not belong to the specified coach");

                if (availability.Status != CoachAvailabilityStatus.Available)
                    throw new CoachAvailabilityNotAvailableException("Availability is not available for booking");

                // 4. Bounds check: all 30-min blocks covering [startTime, endTime) must exist and be Available.
                var coveringBlocks = await availabilityRepo.GetBlocksInRangeForUpdateAsync(coachId, startTime, endTime);
                var availableBlocks = coveringBlocks
                    .Where(b => b.Status == CoachAvailabilityStatus.Available)
                    .OrderBy(b => b.StartTime)
                    .ToList();

                // Verify the blocks fully cover [startTime, endTime) with no gaps
                var cursor = startTime;
                foreach (var block in availableBlocks)
                {
                    if (block.StartTime > cursor)
                        break; // gap detected
                    if (block.EndTime > cursor)
                        cursor = block.EndTime;
                }

                if (cursor < endTime)
                    throw new BadRequestException(
                        $"The requested time range ({startTime:g} - {endTime:g}) " +
                        $"is not fully covered by available slots for this coach");

                // 5. Create BookingRequest (Direct)
                Domain.Entities.BookingRequest br = new()
                {
                    Id = Guid.NewGuid(),
                    CandidateId = candidateId,
                    CoachId = coachId,
                    Type = BookingRequestType.Direct,
                    Status = (paymentAmount == 0) ? BookingRequestStatus.Paid : BookingRequestStatus.Accepted,
                    CoachInterviewServiceId = coachInterviewServiceId,
                    TotalAmount = paymentAmount,
                    ExpiresAt = DateTime.UtcNow.AddHours(24),
                    CreatedAt = DateTime.UtcNow
                };

                // 6. Create 1 InterviewRound linked to the BookingRequest
                var requiredBlockCount = (duration + (AvailabilityBlockMinutes - 1)) / AvailabilityBlockMinutes;
                var roundBlocks = availableBlocks
                    .Where(b => b.StartTime >= startTime && b.StartTime < endTime)
                    .OrderBy(b => b.StartTime)
                    .Take(requiredBlockCount)
                    .ToList();

                var round = new InterviewRound
                {
                    Id = Guid.NewGuid(),
                    BookingRequestId = br.Id,
                    CoachInterviewServiceId = coachInterviewServiceId,
                    RoundNumber = 1,
                    StartTime = startTime,
                    EndTime = endTime,
                    Price = paymentAmount
                };

                // Mark availability blocks as Booked and link to this round
                foreach (var block in roundBlocks)
                {
                    block.Status = CoachAvailabilityStatus.Booked;
                    block.InterviewRoundId = round.Id;
                    availabilityRepo.UpdateAsync(block);
                }

                br.Rounds.Add(round);

                var brRepo = _unitOfWork.GetRepository<IBookingRequestRepository>();
                await brRepo.AddAsync(br);

                // 7. Create Payment + Payout transactions linked to BookingRequest
                InterviewBookingTransaction t = new()
                {
                    OrderCode = RandomGenerator.GenerateOrderCode(),
                    UserId = candidateId,
                    Amount = paymentAmount,
                    Status = TransactionStatus.Created,
                    Type = TransactionType.Payment,
                    BookingRequestId = br.Id,
                };

                InterviewBookingTransaction t2 = new()
                {
                    OrderCode = RandomGenerator.GenerateOrderCode(),
                    UserId = coachId,
                    Amount = paymentAmount,
                    Status = TransactionStatus.Created,
                    Type = TransactionType.Payout,
                    BookingRequestId = br.Id,
                };

                await transactionRepo.AddAsync(t);
                await transactionRepo.AddAsync(t2);

                // 8. Payment gateway or immediate finalization
                string? checkoutUrl = null;
                if (t.Amount == 0)
                {
                    t.Status = TransactionStatus.Paid;
                    t2.Status = TransactionStatus.Paid;

                    var evaluation = await CreateEvaluationResultsFromInterviewService(coachInterviewServiceId);

                    // Use the first availability block as the reference
                    var firstBlockId = roundBlocks.FirstOrDefault()?.Id;

                    var roomRepo = _unitOfWork.GetRepository<IInterviewRoomRepository>();
                    var room = new Domain.Entities.InterviewRoom()
                    {
                        CandidateId = candidateId,
                        CoachId = coachId,
                        ScheduledTime = startTime,
                        DurationMinutes = duration,
                        Status = InterviewRoomStatus.Scheduled,
                        CurrentAvailabilityId = firstBlockId,
                        TransactionId = t.Id,
                        BookingRequestId = br.Id,
                        CoachInterviewServiceId = coachInterviewServiceId,
                        AimLevel = null,
                        RoundNumber = 1,
                        EvaluationResults = evaluation,
                        IsEvaluationCompleted = false
                    };
                    await roomRepo.AddAsync(room);
                    round.InterviewRoomId = room.Id;

                    // Notify Candidate and Coach about successful booking (Free interview)
                    _jobService.Enqueue<INotificationUseCase>(uc => uc.CreateAsync(
                        candidateId,
                        NotificationType.BookingAccepted,
                        "Booking confirmed",
                        "Your interview has been booked successfully.",
                        "/interview?tab=upcoming",
                        null
                    ));

                    _jobService.Enqueue<INotificationUseCase>(uc => uc.CreateAsync(
                        coachId,
                        NotificationType.BookingNew,
                        "New interview scheduled",
                        "A candidate has booked an interview with you.",
                        "/interview?tab=upcoming",
                        null
                    ));
                }
                else
                {
                    checkoutUrl = await _paymentService.CreatePaymentOrderAsync(
                        t.OrderCode,
                        t.Amount,
                        "Book interview",
                        returnUrl,
                        4
                    );
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return checkoutUrl;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        private async Task<List<EvaluationResult>> CreateEvaluationResultsFromInterviewService(Guid? coachInterviewServiceId)
        {
            if (coachInterviewServiceId == null)
                return [];

            var service = await _coachInterviewServiceRepository.GetByIdWithDetailsAsync(coachInterviewServiceId.Value);

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
    }
}

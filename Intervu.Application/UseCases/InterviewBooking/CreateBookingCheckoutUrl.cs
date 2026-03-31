using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
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

                // 3. Validate the availability slot exists, belongs to coach, and is Available
                // Lock the selected availability row so concurrent requests for the same slot
                // cannot both pass the overlap check before one booking is persisted.
                var availability = await availabilityRepo.GetByIdForUpdateAsync(coachAvailabilityId)
                    ?? throw new NotFoundException("Coach availability not found");

                if (availability.CoachId != coachId)
                    throw new BadRequestException("Coach availability does not belong to the specified coach");

                if (availability.Status != CoachAvailabilityStatus.Available)
                    throw new CoachAvailabilityNotAvailableException("Availability is not available for booking");

                // 4. Bounds check: [startTime, endTime] must fit within the availability window
                if (startTime < availability.StartTime || endTime > availability.EndTime)
                    throw new BadRequestException(
                        $"The requested time range ({startTime:g} - {endTime:g}) " +
                        $"does not fit within the availability slot ({availability.StartTime:g} - {availability.EndTime:g})");

                // 5. Overlap check: no existing active booking may collide with the requested range
                var hasOverlap = await transactionRepo.HasOverlappingBookingAsync(coachId, startTime, endTime);
                if (hasOverlap)
                    throw new BadRequestException("This time slot has already been booked.");

                // 6. Create Payment + Payout transactions referencing the original availability
                InterviewBookingTransaction t = new()
                {
                    OrderCode = RandomGenerator.GenerateOrderCode(),
                    UserId = candidateId,
                    Amount = paymentAmount,
                    Status = TransactionStatus.Created,
                    Type = TransactionType.Payment,
                    CoachAvailabilityId = coachAvailabilityId,
                    CoachId = coachId,
                    BookedStartTime = startTime,
                    BookedDurationMinutes = duration,
                };

                InterviewBookingTransaction t2 = new()
                {
                    OrderCode = RandomGenerator.GenerateOrderCode(),
                    UserId = coachId,
                    Amount = paymentAmount,
                    Status = TransactionStatus.Created,
                    Type = TransactionType.Payout,
                    CoachAvailabilityId = coachAvailabilityId,
                    CoachId = coachId,
                    BookedStartTime = startTime,
                    BookedDurationMinutes = duration,
                };

                await transactionRepo.AddAsync(t);
                await transactionRepo.AddAsync(t2);

                // 7. Payment gateway or immediate finalization
                string? checkoutUrl = null;
                if (t.Amount == 0)
                {
                    t.Status = TransactionStatus.Paid;
                    t2.Status = TransactionStatus.Paid;

                    //uc => uc.Ad(candidateId, coachId, coachAvailabilityId, startTime, t.Id, duration);
                    var evaluation = await CreateEvaluationResultsFromInterviewService(coachInterviewServiceId);

                    _jobService.Enqueue<ICreateInterviewRoom>(
                        uc => uc.ExecuteAsync(new Domain.Entities.InterviewRoom()
                        {
                            CandidateId = candidateId,
                            CoachId = coachId,
                            ScheduledTime = startTime,
                            DurationMinutes = duration,
                            Status = InterviewRoomStatus.Scheduled,
                            CurrentAvailabilityId = coachAvailabilityId,
                            TransactionId = t.Id,
                            BookingRequestId = null,
                            CoachInterviewServiceId =coachInterviewServiceId,
                            AimLevel = null,
                            EvaluationResults = evaluation,
                            IsEvaluationCompleted = false
                        })
                    );
                    
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

                // 8. Single save — no split/delete, no EF fixup issues
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

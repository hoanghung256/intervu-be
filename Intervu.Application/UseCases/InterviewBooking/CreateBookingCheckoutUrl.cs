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
    internal class CreateBookingCheckoutUrl : ICreateBookingCheckoutUrl
    {
        private readonly ILogger<CreateBookingCheckoutUrl> _logger;
        private readonly IPaymentService _paymentService;
        private readonly IBackgroundService _jobService;
        private readonly IUnitOfWork _unitOfWork;

        public CreateBookingCheckoutUrl(
            ILogger<CreateBookingCheckoutUrl> logger,
            IPaymentService paymentService,
            IBackgroundService jobService,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _paymentService = paymentService;
            _jobService = jobService;
            _unitOfWork = unitOfWork;
        }

        public async Task<string?> ExecuteAsync(Guid candidateId, Guid coachId, Guid coachAvailabilityId, string returnUrl)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var availabilityRepo = _unitOfWork.GetRepository<ICoachAvailabilitiesRepository>();
                var transactionRepo = _unitOfWork.GetRepository<ITransactionRepository>();
                var coachRepo = _unitOfWork.GetRepository<ICoachProfileRepository>();

                var availability = await availabilityRepo.GetByIdAsync(coachAvailabilityId) ?? throw new NotFoundException("Coach availability not found");

                if (availability.CoachId != coachId) throw new Exception("Coach availability does not belong to the specified coach");

                if (availability.Status != CoachAvailabilityStatus.Available)
                    throw new CoachAvailabilityNotAvailableException("Availability is not available for booking");

                var coach = await coachRepo.GetProfileByIdAsync(coachId) ?? throw new NotFoundException("Interviewer not found");

                // TODO: In Step 4, price will come from CoachInterviewService instead of hardcoded
                int paymentAmount = 2000;
                int duration = (int)(availability.EndTime - availability.StartTime).TotalMinutes;

                // Mark availability as unavailable during booking process
                availability.Status = CoachAvailabilityStatus.Unavailable;

                // Create payment and payout transactions with status 'Created'
                InterviewBookingTransaction t = new()
                {
                    OrderCode = RandomGenerator.GenerateOrderCode(),
                    UserId = candidateId,
                    Amount = paymentAmount,
                    Status = TransactionStatus.Created,
                    Type = TransactionType.Payment,
                    CoachAvailabilityId = coachAvailabilityId,
                };

                InterviewBookingTransaction t2 = new()
                {
                    OrderCode = RandomGenerator.GenerateOrderCode(),
                    UserId = coachId,
                    Amount = paymentAmount,
                    Status = TransactionStatus.Created,
                    Type = TransactionType.Payout,
                    CoachAvailabilityId = coachAvailabilityId,
                };

                await transactionRepo.AddAsync(t);
                await transactionRepo.AddAsync(t2);

                // No payment required: finalize booking immediately
                string? checkoutUrl = null;
                if (t.Amount == 0)
                {
                    availability.Status = CoachAvailabilityStatus.Unavailable;
                    t.Status = TransactionStatus.Paid;
                    t2.Status = TransactionStatus.Paid;

                    _jobService.Enqueue<ICreateInterviewRoom>(
                        uc => uc.ExecuteAsync(candidateId, coachId, availability.Id, availability.StartTime, t.Id, duration)
                    );
                } 
                else
                {
                    // Create PAYOS payment order and get checkout URL
                    checkoutUrl = await _paymentService.CreatePaymentOrderAsync(
                        t.OrderCode,
                        t.Amount,
                        $"Book interview",
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
    }
}

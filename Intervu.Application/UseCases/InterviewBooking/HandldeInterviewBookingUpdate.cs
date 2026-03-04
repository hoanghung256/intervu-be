using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Intervu.Application.UseCases.InterviewBooking
{
    public class HandldeInterviewBookingUpdate : IHandldeInterviewBookingUpdate
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IPaymentService _paymentService;
        private readonly ICoachAvailabilitiesRepository _coachAvailabilitiesRepository;
        private readonly IBackgroundService _backgroundService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<HandldeInterviewBookingUpdate> _logger;

        public HandldeInterviewBookingUpdate(
            ITransactionRepository transactionRepository,
            IPaymentService paymentService,
            ICoachAvailabilitiesRepository coachAvailabilitiesRepository,
            IBackgroundService backgroundService,
            IUnitOfWork unitOfWork,
            ILogger<HandldeInterviewBookingUpdate> logger)
        {
            _transactionRepository = transactionRepository;
            _paymentService = paymentService;
            _coachAvailabilitiesRepository = coachAvailabilitiesRepository;
            _backgroundService = backgroundService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task ExecuteAsync(object webhookPayload)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var (isValid, orderCode) = _paymentService.VerifyPayment(webhookPayload);

                if (!isValid) return;

                var transactionRepo = _unitOfWork.GetRepository<ITransactionRepository>();

                InterviewBookingTransaction transaction = await transactionRepo.Get(orderCode, TransactionType.Payment)
                    ?? throw new NotFoundException("Booking transaction not found");

                transaction.Status = TransactionStatus.Paid;

                // --- Flow B/C: BookingRequest payment ---
                if (transaction.BookingRequestId != null)
                {
                    await HandleBookingRequestPayment(transaction);
                }
                // --- Flow A: Normal availability booking ---
                else if (transaction.CoachAvailabilityId != null)
                {
                    await HandleAvailabilityPayment(transaction);
                }
                else
                {
                    throw new NotFoundException("Transaction has no associated booking context");
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        /// <summary>
        /// Flow A: Normal booking via coach availability slot
        /// </summary>
        private async Task HandleAvailabilityPayment(InterviewBookingTransaction transaction)
        {
            var availabilityRepo = _unitOfWork.GetRepository<ICoachAvailabilitiesRepository>();

            CoachAvailability availability = await availabilityRepo.GetByIdAsync(transaction.CoachAvailabilityId!.Value)
                ?? throw new NotFoundException("Coach availability not found");

            if (availability.Status != CoachAvailabilityStatus.Unavailable)
                throw new CoachAvailabilityNotAvailableException("Availability is not in booking state");

            availability.Status = CoachAvailabilityStatus.Unavailable;

            int durationMinutes = (int)(availability.EndTime - availability.StartTime).TotalMinutes;

            _backgroundService.Enqueue<ICreateInterviewRoom>(
                uc => uc.ExecuteAsync(transaction.UserId, availability.CoachId, availability.Id, availability.StartTime, transaction.Id, durationMinutes)
            );
        }

        /// <summary>
        /// Flow B/C: BookingRequest payment — mark as Paid and create interview rooms
        /// </summary>
        private async Task HandleBookingRequestPayment(InterviewBookingTransaction transaction)
        {
            var bookingRepo = _unitOfWork.GetRepository<IBookingRequestRepository>();
            var roomRepo = _unitOfWork.GetRepository<IInterviewRoomRepository>();

            var bookingRequest = await bookingRepo.GetByIdWithDetailsAsync(transaction.BookingRequestId!.Value)
                ?? throw new NotFoundException("Booking request not found");

            if (bookingRequest.Status != BookingRequestStatus.Accepted)
            {
                _logger.LogWarning(
                    "BookingRequest {Id} is not in Accepted status (current: {Status}), skipping room creation",
                    bookingRequest.Id, bookingRequest.Status);
                return;
            }

            // Transition to Paid
            bookingRequest.Status = BookingRequestStatus.Paid;
            bookingRequest.UpdatedAt = DateTime.UtcNow;
            bookingRepo.UpdateAsync(bookingRequest);

            if (bookingRequest.Type == BookingRequestType.External)
            {
                // Flow B: Create a single interview room
                var room = new Domain.Entities.InterviewRoom
                {
                    CandidateId = bookingRequest.CandidateId,
                    CoachId = bookingRequest.CoachId,
                    ScheduledTime = bookingRequest.RequestedStartTime,
                    DurationMinutes = bookingRequest.CoachInterviewService?.DurationMinutes ?? 60,
                    Status = InterviewRoomStatus.Scheduled,
                    TransactionId = transaction.Id,
                    BookingRequestId = bookingRequest.Id,
                    CoachInterviewServiceId = bookingRequest.CoachInterviewServiceId,
                    AimLevel = bookingRequest.AimLevel,
                };
                await roomRepo.AddAsync(room);

                _logger.LogInformation(
                    "Created interview room for external BookingRequest {BookingRequestId}",
                    bookingRequest.Id);
            }
            else if (bookingRequest.Type == BookingRequestType.JDInterview)
            {
                // Flow C: Create one room per interview round
                foreach (var round in bookingRequest.Rounds.OrderBy(r => r.RoundNumber))
                {
                    var room = new Domain.Entities.InterviewRoom
                    {
                        CandidateId = bookingRequest.CandidateId,
                        CoachId = bookingRequest.CoachId,
                        ScheduledTime = round.StartTime,
                        DurationMinutes = round.CoachInterviewService?.DurationMinutes ?? 60,
                        Status = InterviewRoomStatus.Scheduled,
                        TransactionId = transaction.Id,
                        BookingRequestId = bookingRequest.Id,
                        CoachInterviewServiceId = round.CoachInterviewServiceId,
                        AimLevel = bookingRequest.AimLevel,
                        RoundNumber = round.RoundNumber,
                    };
                    await roomRepo.AddAsync(room);
                }

                _logger.LogInformation(
                    "Created {RoundCount} interview rooms for JD BookingRequest {BookingRequestId}",
                    bookingRequest.Rounds.Count, bookingRequest.Id);
            }
        }
    }
}
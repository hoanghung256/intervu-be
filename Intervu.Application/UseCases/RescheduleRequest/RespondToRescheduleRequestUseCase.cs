using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.UseCases.RescheduleRequest;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Intervu.Application.UseCases.RescheduleRequest
{
    internal class RespondToRescheduleRequestUseCase : IRespondToRescheduleRequestUseCase
    {
        private readonly ILogger<RespondToRescheduleRequestUseCase> _logger;
        private readonly IRescheduleRequestRepository _rescheduleRequestRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICoachAvailabilitiesRepository _coachAvailabilitiesRepository;

        public RespondToRescheduleRequestUseCase(
            ILogger<RespondToRescheduleRequestUseCase> logger,
            IRescheduleRequestRepository rescheduleRequestRepository,
            ITransactionRepository transactionRepository,
            ICoachAvailabilitiesRepository coachAvailabilitiesRepository)
        {
            _logger = logger;
            _rescheduleRequestRepository = rescheduleRequestRepository;
            _transactionRepository = transactionRepository;
            _coachAvailabilitiesRepository = coachAvailabilitiesRepository;
        }

        public async Task ExecuteAsync(Guid requestId, Guid respondedBy, bool isApproved, string? rejectionReason)
        {
            var request = await _rescheduleRequestRepository.GetByIdAsync(requestId);
            if(request == null)
            {
                _logger.LogWarning("Reschedule request with ID {RequestId} not found.", requestId);
                throw new NotFoundException("Reschedule request not found");
            }

            if(request.Status != RescheduleRequestStatus.Pending)
            {
                _logger.LogWarning("Reschedule request with ID {RequestId} is not pending.", requestId);
                throw new ConflictException("Reschedule request is not pending");
            }

            if(request.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("Reschedule request with ID {RequestId} has expired.", requestId);
                throw new ConflictException("Reschedule request has expired");
            }

            if (isApproved)
            {
                request.Status = RescheduleRequestStatus.Approved;
                var booking = await _transactionRepository.GetByIdAsync(request.InterviewBookingTransactionId);
                if (booking == null)
                {
                    _logger.LogWarning("Booking with ID {BookingId} not found.", request.InterviewBookingTransactionId);
                    throw new NotFoundException("Booking not found");
                }
                // Chỉ update CoachAvailabilityId, không gọi UpdateAsync để tránh update OrderCode
                booking.CoachAvailabilityId = request.ProposedAvailabilityId;
                await _transactionRepository.SaveChangesAsync();

                var proposedAvailability = await _coachAvailabilitiesRepository.GetByIdAsync(request.ProposedAvailabilityId);
                if (proposedAvailability == null)
                {
                    _logger.LogWarning("Proposed availability with ID {ProposedAvailabilityId} not found.", request.ProposedAvailabilityId);
                    throw new NotFoundException("Proposed availability not found");
                }
                proposedAvailability.Status = CoachAvailabilityStatus.Booked;
                await _coachAvailabilitiesRepository.SaveChangesAsync();

                var currentAvailability = await _coachAvailabilitiesRepository.GetByIdAsync(request.CurrentAvailabilityId);
                if (currentAvailability == null)
                {
                    _logger.LogWarning("Current availability with ID {CurrentAvailabilityId} not found.", request.CurrentAvailabilityId);
                    throw new NotFoundException("Current availability not found");
                }
                currentAvailability.Status = CoachAvailabilityStatus.Available;
                await _coachAvailabilitiesRepository.SaveChangesAsync();

                request.RespondedAt = DateTime.UtcNow;
                request.RespondedBy = respondedBy;
                await _rescheduleRequestRepository.SaveChangesAsync();
            }

            if (!isApproved)
            {
                request.Status = RescheduleRequestStatus.Rejected;
                request.RejectionReason = rejectionReason;
                request.RespondedAt = DateTime.UtcNow;
                request.RespondedBy = respondedBy;
                await _rescheduleRequestRepository.SaveChangesAsync();
            }
        }
    }
}

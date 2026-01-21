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
            var request = await _rescheduleRequestRepository.GetByIdWithDetailsAsync(requestId);
            if (request == null)
            {
                _logger.LogWarning("Reschedule request with ID {RequestId} not found.", requestId);
                throw new NotFoundException("Reschedule request not found");
            }

            if (request.Status != RescheduleRequestStatus.Pending)
            {
                _logger.LogWarning("Reschedule request with ID {RequestId} is not pending.", requestId);
                throw new ConflictException("Reschedule request is not pending");
            }

            if (request.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("Reschedule request with ID {RequestId} has expired.", requestId);
                throw new ConflictException("Reschedule request has expired");
            }

            if (request.Booking == null)
            {
                _logger.LogWarning("Booking not found for reschedule request {RequestId}.", requestId);
                throw new NotFoundException("Booking not found");
            }

            var currentAvailability = await _coachAvailabilitiesRepository.GetByIdAsync(request.CurrentAvailabilityId);
            if (currentAvailability == null)
            {
                _logger.LogWarning("Current availability with ID {CurrentAvailabilityId} not found.", request.CurrentAvailabilityId);
                throw new NotFoundException("Current availability not found");
            }

            bool isRequesterCandidate = request.RequestedBy == request.Booking.UserId;
            bool isRequesterCoach = request.RequestedBy == currentAvailability.CoachId;

            bool isResponderCandidate = respondedBy == request.Booking.UserId;
            bool isResponderCoach = respondedBy == currentAvailability.CoachId;

            if (isRequesterCandidate && !isResponderCoach)
            {
                _logger.LogWarning("Candidate requested reschedule, but responder {RespondedBy} is not the coach.", respondedBy);
                throw new ForbiddenException("Only the coach can respond to this reschedule request");
            }

            if (isRequesterCoach && !isResponderCandidate)
            {
                _logger.LogWarning("Coach requested reschedule, but responder {RespondedBy} is not the candidate.", respondedBy);
                throw new ForbiddenException("Only the candidate can respond to this reschedule request");
            }

            if (!isResponderCandidate && !isResponderCoach)
            {
                _logger.LogWarning("Responder {RespondedBy} is neither the coach nor the candidate.", respondedBy);
                throw new ForbiddenException("You are not authorized to respond to this request");
            }

            if (isApproved)
            {
                var proposedAvailability = await _coachAvailabilitiesRepository.GetByIdAsync(request.ProposedAvailabilityId);
                if (proposedAvailability == null)
                {
                    _logger.LogWarning("Proposed availability with ID {ProposedAvailabilityId} not found.", request.ProposedAvailabilityId);
                    throw new NotFoundException("Proposed availability not found");
                }

                request.Booking.CoachAvailabilityId = request.ProposedAvailabilityId;
                _transactionRepository.UpdateAsync(request.Booking);

                proposedAvailability.Status = CoachAvailabilityStatus.Booked;
                _coachAvailabilitiesRepository.UpdateAsync(proposedAvailability);

                currentAvailability.Status = CoachAvailabilityStatus.Available;
                _coachAvailabilitiesRepository.UpdateAsync(currentAvailability);

                request.Status = RescheduleRequestStatus.Approved;
                request.RespondedAt = DateTime.UtcNow;
                request.RespondedBy = respondedBy;
                _rescheduleRequestRepository.UpdateAsync(request);

                await _transactionRepository.SaveChangesAsync();
                await _coachAvailabilitiesRepository.SaveChangesAsync();
                await _rescheduleRequestRepository.SaveChangesAsync();

                _logger.LogInformation("Reschedule request {RequestId} approved by {RespondedBy}", requestId, respondedBy);
            }
            else
            {
                request.Status = RescheduleRequestStatus.Rejected;
                request.RejectionReason = rejectionReason;
                request.RespondedAt = DateTime.UtcNow;
                request.RespondedBy = respondedBy;
                _rescheduleRequestRepository.UpdateAsync(request);

                await _rescheduleRequestRepository.SaveChangesAsync();

                _logger.LogInformation("Reschedule request {RequestId} rejected by {RespondedBy}", requestId, respondedBy);
            }
        }
    }
}

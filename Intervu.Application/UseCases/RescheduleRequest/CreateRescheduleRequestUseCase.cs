using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.UseCases.RescheduleRequest;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Intervu.Application.UseCases.RescheduleRequest
{
    internal class CreateRescheduleRequestUseCase : ICreateRescheduleRequestUseCase
    {
        private readonly ILogger<CreateRescheduleRequestUseCase> _logger;
        private readonly IRescheduleRequestRepository _rescheduleRequestRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICoachAvailabilitiesRepository _coachAvailabilitiesRepository;
        private readonly IUserRepository _userRepository;

        public CreateRescheduleRequestUseCase(
            ILogger<CreateRescheduleRequestUseCase> logger,
            IRescheduleRequestRepository rescheduleRequestRepository,
            ITransactionRepository transactionRepository,
            ICoachAvailabilitiesRepository coachAvailabilitiesRepository,
            IUserRepository userRepository)
        {
            _logger = logger;
            _rescheduleRequestRepository = rescheduleRequestRepository;
            _transactionRepository = transactionRepository;
            _coachAvailabilitiesRepository = coachAvailabilitiesRepository;
            _userRepository = userRepository;
        }

        public async Task<Guid> ExecuteAsync(Guid bookingId, Guid proposedAvailabilityId, Guid requestedBy, string reason)
        {
            var booking = await _transactionRepository.GetByIdAsync(bookingId);
            if (booking == null)
            {
                _logger.LogWarning("Booking with ID {BookingId} not found.", bookingId);
                throw new NotFoundException("Booking not found");
            }

            var currentAvailability = await _coachAvailabilitiesRepository.GetByIdAsync(booking.CoachAvailabilityId);
            if (currentAvailability == null)
            {
                _logger.LogWarning("Current availability with ID {AvailabilityId} not found.", booking.CoachAvailabilityId);
                throw new NotFoundException("Current availability not found");
            }

            var proposedAvailability = await _coachAvailabilitiesRepository.GetByIdAsync(proposedAvailabilityId);
            if (proposedAvailability == null)
            {
                _logger.LogWarning("Proposed availability with ID {ProposedAvailabilityId} not found.", proposedAvailabilityId);
                throw new NotFoundException("Proposed availability not found");
            }

            if (proposedAvailability.Status != CoachAvailabilityStatus.Available)
            {
                _logger.LogWarning("Proposed availability with ID {ProposedAvailabilityId} is not available.", proposedAvailabilityId);
                throw new CoachAvailabilityNotAvailableException("Proposed availability is not available for booking");
            }

            var existedRequest = await _rescheduleRequestRepository.GetPendingRequestByBookingIdAsync(bookingId);
            if (existedRequest != null)
            {
                _logger.LogWarning("A pending reschedule request already exists for booking ID {BookingId}.", bookingId);
                throw new ConflictException("A pending reschedule request already exists for this booking");
            }

            var requester = await _userRepository.GetByIdAsync(requestedBy);
            if (requester == null)
            {
                _logger.LogWarning("User with ID {RequestedBy} not found.", requestedBy);
                throw new NotFoundException("User not found");
            }

            bool isCandidate = requester.Id == booking.UserId;
            bool isCoach = requester.Id == currentAvailability.CoachId;

            if (!isCandidate && !isCoach)
            {
                _logger.LogWarning("User with ID {RequestedBy} is neither the coach nor the candidate for booking ID {BookingId}.", requestedBy, bookingId);
                throw new ForbiddenException("You are not authorized to reschedule this booking");
            }

            var rescheduleRequest = new InterviewRescheduleRequest
            {
                Id = Guid.NewGuid(),
                InterviewBookingTransactionId = booking.Id,
                CurrentAvailabilityId = booking.CoachAvailabilityId,
                ProposedAvailabilityId = proposedAvailability.Id,
                RequestedBy = requester.Id,
                Reason = reason,
                Status = RescheduleRequestStatus.Pending,
                ExpiresAt = DateTime.UtcNow.AddDays(2)
            };

            await _rescheduleRequestRepository.AddAsync(rescheduleRequest);
            await _rescheduleRequestRepository.SaveChangesAsync();

            _logger.LogInformation("Created reschedule request {RequestId} for booking {BookingId}", rescheduleRequest.Id, bookingId);
            return rescheduleRequest.Id;
        }
    }
}

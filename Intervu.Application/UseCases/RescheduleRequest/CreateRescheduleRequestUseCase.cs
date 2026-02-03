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
        private readonly IInterviewRoomRepository _interviewRoomRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICoachAvailabilitiesRepository _coachAvailabilitiesRepository;
        private readonly IUserRepository _userRepository;

        public CreateRescheduleRequestUseCase(
            ILogger<CreateRescheduleRequestUseCase> logger,
            IRescheduleRequestRepository rescheduleRequestRepository,
            IInterviewRoomRepository interviewRoomRepository,
            ITransactionRepository transactionRepository,
            ICoachAvailabilitiesRepository coachAvailabilitiesRepository,
            IUserRepository userRepository)
        {
            _logger = logger;
            _rescheduleRequestRepository = rescheduleRequestRepository;
            _interviewRoomRepository = interviewRoomRepository;
            _transactionRepository = transactionRepository;
            _coachAvailabilitiesRepository = coachAvailabilitiesRepository;
            _userRepository = userRepository;
        }

        public async Task<Guid> ExecuteAsync(Guid roomId, Guid proposedAvailabilityId, Guid requestedBy, string reason)
        {
            var room = await _interviewRoomRepository.GetByIdAsync(roomId);
            if (room == null)
            {
                _logger.LogWarning("Interview room with ID {RoomId} not found.", roomId);
                throw new NotFoundException("Interview room not found");
            }

            if (room.CoachId == null)
            {
                _logger.LogWarning("Interview room {RoomId} has no coach assigned.", roomId);
                throw new ConflictException("Interview room has no coach assigned");
            }

            if (room.ScheduledTime == null)
            {
                _logger.LogWarning("Interview room {RoomId} has no scheduled time.", roomId);
                throw new ConflictException("Interview room has no scheduled time");
            }

            if (room.TransactionId == null)
            {
                _logger.LogWarning("Interview room {RoomId} has no transaction.", roomId);
                throw new ConflictException("Interview room has no transaction");
            }

            // BR-05: 12-hour rule - Cannot reschedule within 12 hours of scheduled time
            var timeUntilInterview = room.ScheduledTime.Value - DateTime.UtcNow;
            if (timeUntilInterview < TimeSpan.FromHours(12))
            {
                _logger.LogWarning("Cannot reschedule room {RoomId} within 12 hours of scheduled time. Time remaining: {TimeRemaining}", 
                    roomId, timeUntilInterview);
                throw new ConflictException("Cannot reschedule within 12 hours of the scheduled interview time. Please cancel if you cannot attend.");
            }

            // BR-05: Limit 1 reschedule attempt per interview
            if (room.RescheduleAttemptCount >= 1)
            {
                _logger.LogWarning("Room {RoomId} has already been rescheduled {Count} time(s). Maximum attempts reached.", 
                    roomId, room.RescheduleAttemptCount);
                throw new ConflictException("This interview has already been rescheduled once. Only cancellation is allowed.");
            }

            // Get current booking transaction
            var currentTransaction = await _transactionRepository.GetByIdAsync(room.TransactionId.Value);
            if (currentTransaction == null)
            {
                _logger.LogWarning("Transaction {TransactionId} not found for room {RoomId}.", room.TransactionId, roomId);
                throw new NotFoundException("Transaction not found for this interview room");
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

            // Validation: Proposed time must be in the future
            if (proposedAvailability.StartTime <= DateTime.UtcNow)
            {
                _logger.LogWarning("Proposed availability {ProposedAvailabilityId} has start time in the past: {StartTime}", 
                    proposedAvailabilityId, proposedAvailability.StartTime);
                throw new ConflictException("Proposed time must be in the future");
            }

            // Proposed availability must be different from current
            if (currentTransaction.CoachAvailabilityId == proposedAvailabilityId)
            {
                _logger.LogWarning("Proposed availability {ProposedAvailabilityId} is the same as current availability.", proposedAvailabilityId);
                throw new ConflictException("Proposed availability must be different from the current scheduled time");
            }

            // Validation: Check if sender has conflicting interview sessions at proposed time
            var conflictingRooms = await _interviewRoomRepository.GetConflictingRoomsAsync(
                requestedBy, 
                proposedAvailability.StartTime, 
                proposedAvailability.EndTime);
            
            if (conflictingRooms.Any())
            {
                _logger.LogWarning("User {UserId} has conflicting interview sessions at proposed time {StartTime}-{EndTime}", 
                    requestedBy, proposedAvailability.StartTime, proposedAvailability.EndTime);
                throw new ConflictException("The proposed time conflicts with your existing interview sessions");
            }

            var existedRequest = await _rescheduleRequestRepository.GetPendingRequestByRoomIdAsync(roomId);
            if (existedRequest != null)
            {
                _logger.LogWarning("A pending reschedule request already exists for room ID {RoomId}.", roomId);
                throw new ConflictException("A pending reschedule request already exists for this interview room");
            }

            var requester = await _userRepository.GetByIdAsync(requestedBy);
            if (requester == null)
            {
                _logger.LogWarning("User with ID {RequestedBy} not found.", requestedBy);
                throw new NotFoundException("User not found");
            }

            bool isCandidate = requester.Id == room.CandidateId;
            bool isCoach = requester.Id == room.CoachId;

            if (!isCandidate && !isCoach)
            {
                _logger.LogWarning("User with ID {RequestedBy} is neither the coach nor the candidate for room ID {RoomId}.", requestedBy, roomId);
                throw new ForbiddenException("You are not authorized to reschedule this interview");
            }

            var rescheduleRequest = new InterviewRescheduleRequest
            {
                Id = Guid.NewGuid(),
                InterviewRoomId = room.Id,
                CurrentAvailabilityId = currentTransaction.CoachAvailabilityId,
                ProposedAvailabilityId = proposedAvailability.Id,
                RequestedBy = requester.Id,
                Reason = reason,
                Status = RescheduleRequestStatus.Pending,
                ExpiresAt = DateTime.UtcNow.AddDays(2)
            };

            await _rescheduleRequestRepository.AddAsync(rescheduleRequest);
            await _rescheduleRequestRepository.SaveChangesAsync();

            _logger.LogInformation("Created reschedule request {RequestId} for room {RoomId}", rescheduleRequest.Id, roomId);
            return rescheduleRequest.Id;
        }
    }
}

using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.ExternalServices.Email;
using Intervu.Application.Interfaces.UseCases.Notification;
using Intervu.Application.Interfaces.UseCases.RescheduleRequest;
using Intervu.Application.Services;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Intervu.Application.UseCases.RescheduleRequest
{
    // TODO: Implement approval/rejection flow — currently auto-approved
    internal class CreateRescheduleRequestUseCase : ICreateRescheduleRequestUseCase
    {
        private readonly ILogger<CreateRescheduleRequestUseCase> _logger;
        private readonly IRescheduleRequestRepository _rescheduleRequestRepository;
        private readonly IInterviewRoomRepository _interviewRoomRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICoachAvailabilitiesRepository _coachAvailabilitiesRepository;
        private readonly IBookingRequestRepository _bookingRequestRepository;
        private readonly IUserRepository _userRepository;
        private readonly IBackgroundService _backgroundService;
        private readonly IConfiguration _configuration;

        public CreateRescheduleRequestUseCase(
            ILogger<CreateRescheduleRequestUseCase> logger,
            IRescheduleRequestRepository rescheduleRequestRepository,
            IInterviewRoomRepository interviewRoomRepository,
            ITransactionRepository transactionRepository,
            ICoachAvailabilitiesRepository coachAvailabilitiesRepository,
            IBookingRequestRepository bookingRequestRepository,
            IUserRepository userRepository,
            IBackgroundService backgroundService,
            IConfiguration configuration)
        {
            _logger = logger;
            _rescheduleRequestRepository = rescheduleRequestRepository;
            _interviewRoomRepository = interviewRoomRepository;
            _transactionRepository = transactionRepository;
            _coachAvailabilitiesRepository = coachAvailabilitiesRepository;
            _bookingRequestRepository = bookingRequestRepository;
            _userRepository = userRepository;
            _backgroundService = backgroundService;
            _configuration = configuration;
        }

        public async Task<Guid> ExecuteAsync(Guid roomId, DateTime newStartTime, Guid requestedBy, string reason)
        {
            newStartTime = EnsureUtc(newStartTime);

            var room = await _interviewRoomRepository.GetByIdWithDetailsAsync(roomId);
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

            // Use room's built-in validation method (checks 12-hour rule and reschedule count)
            if (!room.IsAvailableForReschedule())
            {
                var timeUntilInterview = room.ScheduledTime.Value - DateTime.UtcNow;

                if (timeUntilInterview < TimeSpan.FromHours(12))
                {
                    _logger.LogWarning("Cannot reschedule room {RoomId} within 12 hours of scheduled time. Time remaining: {TimeRemaining}",
                        roomId, timeUntilInterview);
                    throw new ConflictException("Cannot reschedule within 12 hours of the scheduled interview time. Please cancel if you cannot attend.");
                }

                if (room.RescheduleAttemptCount >= 1)
                {
                    _logger.LogWarning("Room {RoomId} has already been rescheduled {Count} time(s). Maximum attempts reached.",
                        roomId, room.RescheduleAttemptCount);
                    throw new ConflictException("This interview has already been rescheduled once. Only cancellation is allowed.");
                }

                throw new ConflictException("This interview is not available for rescheduling.");
            }

            // Validate proposed time is in the future
            if (newStartTime <= DateTime.UtcNow)
            {
                throw new ConflictException("Proposed time must be in the future");
            }

            // Proposed time must be different from current
            var currentStart = EnsureUtc(room.ScheduledTime.Value);
            if (newStartTime == currentStart)
            {
                throw new ConflictException("Proposed time must be different from the current scheduled time");
            }

            var durationMinutes = room.DurationMinutes ?? 30;
            var proposedEndTime = newStartTime.AddMinutes(durationMinutes);

            // Validate against coach free slots (same pattern as RescheduleJDBookingRequest)
            await ValidateCoachAvailability(room.CoachId.Value, newStartTime, proposedEndTime, room);

            // Validate no conflicts for the requester
            var conflictingRooms = await _interviewRoomRepository.GetConflictingRoomsAsync(
                requestedBy, newStartTime, proposedEndTime);

            // Exclude the current room from conflict check
            if (conflictingRooms.Any(c => c.Id != roomId))
            {
                _logger.LogWarning("User {UserId} has conflicting interview sessions at proposed time {StartTime}-{EndTime}",
                    requestedBy, newStartTime, proposedEndTime);
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

            // Create reschedule request record (auto-approved)
            var rescheduleRequest = new InterviewRescheduleRequest
            {
                Id = Guid.NewGuid(),
                InterviewRoomId = room.Id,
                CurrentAvailabilityId = room.CurrentAvailabilityId,
                ProposedStartTime = newStartTime,
                ProposedEndTime = proposedEndTime,
                RequestedBy = requester.Id,
                Reason = reason,
                // TODO: Implement approval/rejection flow — currently auto-approved
                Status = RescheduleRequestStatus.Approved,
                RespondedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            };

            await _rescheduleRequestRepository.AddAsync(rescheduleRequest);

            // Auto-approve: update room schedule immediately
            room.ScheduledTime = newStartTime;
            room.RescheduleAttemptCount++;
            _interviewRoomRepository.UpdateAsync(room);

            await _rescheduleRequestRepository.SaveChangesAsync();

            // Notify both parties about the completed reschedule
            var otherPartyId = isCandidate ? room.CoachId!.Value : room.CandidateId!.Value;

            _backgroundService.Enqueue<INotificationUseCase>(
                uc => uc.CreateAsync(
                    otherPartyId,
                    NotificationType.RescheduleAccepted,
                    "Interview rescheduled",
                    $"{requester.FullName} has rescheduled the interview.",
                    "/interview?tab=upcoming",
                    rescheduleRequest.Id));

            var recipient = await _userRepository.GetByIdAsync(otherPartyId);
            if (recipient != null)
            {
                try
                {
                    var frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:5173";
                    var placeholders = new Dictionary<string, string>
                    {
                        ["RecipientName"] = recipient.FullName,
                        ["RequesterName"] = requester.FullName,
                        ["Reason"] = reason,
                        ["ProposedTime"] = proposedAvailability.StartTime.ToString("dd MMM yyyy HH:mm"),
                        ["DashboardLink"] = $"{frontendUrl.TrimEnd('/')}/interview?tab=upcoming"
                    };

                    _backgroundService.Enqueue<IEmailService>(svc => svc.SendEmailWithTemplateAsync(
                        recipient.Email,
                        "RescheduleProposal",
                        placeholders));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to enqueue reschedule proposal email for request {RequestId}", rescheduleRequest.Id);
                }
            }

            _logger.LogInformation("Created reschedule request {RequestId} for room {RoomId}", rescheduleRequest.Id, roomId);
            _backgroundService.Enqueue<INotificationUseCase>(
                uc => uc.CreateAsync(
                    requestedBy,
                    NotificationType.RescheduleAccepted,
                    "Reschedule successful",
                    "Your interview has been rescheduled successfully.",
                    "/interview?tab=upcoming",
                    rescheduleRequest.Id));

            _logger.LogInformation("Created and auto-approved reschedule request {RequestId} for room {RoomId}", rescheduleRequest.Id, roomId);
            return rescheduleRequest.Id;
        }

        private async Task ValidateCoachAvailability(Guid coachId, DateTime newStartTime, DateTime proposedEndTime, Domain.Entities.InterviewRoom room)
        {
            var rawAvailabilities = (await _coachAvailabilitiesRepository
                .GetCoachAvailabilitiesByMonthAsync(coachId, 0, 0))
                .ToList();

            if (rawAvailabilities.Count == 0)
            {
                throw new ConflictException("Coach has no available slots to reschedule");
            }

            // Get all booked intervals, excluding the current room's interval
            var currentRoomStart = EnsureUtc(room.ScheduledTime!.Value);
            var currentRoomEnd = currentRoomStart.AddMinutes(room.DurationMinutes ?? 30);

            var rangeStart = rawAvailabilities.Min(a => a.StartTime);
            var rangeEnd = rawAvailabilities.Max(a => a.EndTime);

            var activeRounds = await _bookingRequestRepository
                .GetActiveRoundsByCoachAsync(coachId, rangeStart, rangeEnd);

            // Exclude the current room's booking from the occupied set
            var allBookedIntervals = activeRounds
                .Where(interval => !(EnsureUtc(interval.Start) == currentRoomStart && EnsureUtc(interval.End) == currentRoomEnd))
                .Select(x => (EnsureUtc(x.Start), EnsureUtc(x.End)))
                .ToList();

            var freeSlots = AvailabilityCalculatorService.CalculateFreeSlots(rawAvailabilities, allBookedIntervals);

            var fits = freeSlots.Any(slot => newStartTime >= slot.Start && proposedEndTime <= slot.End);

            if (!fits)
            {
                throw new ConflictException("The proposed time does not fit in coach available slots");
            }
        }

        private static DateTime EnsureUtc(DateTime value)
        {
            if (value.Kind == DateTimeKind.Utc) return value;
            if (value.Kind == DateTimeKind.Unspecified) return DateTime.SpecifyKind(value, DateTimeKind.Utc);
            return value.ToUniversalTime();
        }
    }
}

using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.ExternalServices.Email;
using Intervu.Application.Interfaces.UseCases.Notification;
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
        private readonly IInterviewRoomRepository _interviewRoomRepository;
        private readonly ICoachAvailabilitiesRepository _coachAvailabilitiesRepository;
        private readonly IBackgroundService _backgroundService;
        private readonly IUserRepository _userRepository;
        public RespondToRescheduleRequestUseCase(
            ILogger<RespondToRescheduleRequestUseCase> logger,
            IRescheduleRequestRepository rescheduleRequestRepository,
            IInterviewRoomRepository interviewRoomRepository,
            ICoachAvailabilitiesRepository coachAvailabilitiesRepository,
            IBackgroundService backgroundService,
            IUserRepository userRepository)
        {
            _logger = logger;
            _rescheduleRequestRepository = rescheduleRequestRepository;
            _interviewRoomRepository = interviewRoomRepository;
            _coachAvailabilitiesRepository = coachAvailabilitiesRepository;
            _backgroundService = backgroundService;
            _userRepository = userRepository;
        }

        public async Task ExecuteAsync(Guid requestId, Guid respondedBy, bool isApproved, string? rejectionReason)
        {
            var request = await _rescheduleRequestRepository.GetByIdAsync(requestId);
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

            // Load the interview room to validate authorization
            var room = await _interviewRoomRepository.GetByIdWithDetailsAsync(request.InterviewRoomId);
            if (room == null)
            {
                _logger.LogWarning("Interview room with ID {RoomId} not found.", request.InterviewRoomId);
                throw new NotFoundException("Interview room not found");
            }

            // Responder must be the other party (not the requester)
            if (request.RequestedBy == respondedBy)
            {
                _logger.LogWarning("User {UserId} cannot respond to their own reschedule request {RequestId}.", respondedBy, requestId);
                throw new ForbiddenException("You cannot respond to your own reschedule request");
            }

            // Responder must be either coach or candidate in the room
            bool isResponderInRoom = room.CoachId == respondedBy || room.CandidateId == respondedBy;
            if (!isResponderInRoom)
            {
                _logger.LogWarning("User {UserId} is not authorized to respond to reschedule request {RequestId}.", respondedBy, requestId);
                throw new ForbiddenException("You are not authorized to respond to this reschedule request");
            }

            if (isApproved)
            {
                var approvedTime = string.Empty;
                request.Status = RescheduleRequestStatus.Approved;

                // Room already loaded above
                if (room == null)
                {
                    _logger.LogWarning("Interview room with ID {RoomId} not found.", request.InterviewRoomId);
                    throw new NotFoundException("Interview room not found");
                }

                // Update room scheduled time based on proposed availability
                var proposedAvailability = await _coachAvailabilitiesRepository.GetByIdAsync(request.ProposedAvailabilityId);
                if (proposedAvailability == null)
                {
                    _logger.LogWarning("Proposed availability with ID {ProposedAvailabilityId} not found.", request.ProposedAvailabilityId);
                    throw new NotFoundException("Proposed availability not found");
                }

                // Update CurrentAvailabilityId
                room.CurrentAvailabilityId = request.ProposedAvailabilityId;

                // Keep ScheduledTime in sync for backward compatibility (will be removed in future)
                room.ScheduledTime = proposedAvailability.StartTime;
                approvedTime = proposedAvailability.StartTime.ToString("dd MMM yyyy HH:mm");

                // Increment reschedule attempt count
                room.RescheduleAttemptCount++;

                _interviewRoomRepository.UpdateAsync(room);
                await _interviewRoomRepository.SaveChangesAsync();

                // Update transaction to point to new availability
                if (room.Transaction != null)
                {
                    room.Transaction.CoachAvailabilityId = request.ProposedAvailabilityId;
                    await _interviewRoomRepository.SaveChangesAsync();
                    _logger.LogInformation("Updated transaction {TransactionId} to new availability {AvailabilityId}",
                        room.TransactionId, request.ProposedAvailabilityId);
                }
                
                // Mark proposed availability as unavailable (booked)
                proposedAvailability.Status = CoachAvailabilityStatus.Unavailable;
                await _coachAvailabilitiesRepository.SaveChangesAsync();

                // Release current availability (mark as available again)
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

                // Notify requester — reschedule approved
                _backgroundService.Enqueue<INotificationUseCase>(
                    uc => uc.CreateAsync(
                        request.RequestedBy,
                        NotificationType.RescheduleAccepted,
                        "Reschedule approved",
                        "Your reschedule request has been approved.",
                        "/interview?tab=upcoming",
                        requestId));

                var requester = await _userRepository.GetByIdAsync(request.RequestedBy);
                if (requester != null)
                {
                    var placeholders = new Dictionary<string, string>
                    {
                        ["RecipientName"] = requester.FullName,
                        ["Status"] = "Approved",
                        ["RejectionReason"] = string.Empty,
                        ["NewTime"] = approvedTime
                    };

                    _backgroundService.Enqueue<IEmailService>(svc => svc.SendEmailWithTemplateAsync(
                        requester.Email,
                        "RescheduleResponse",
                        placeholders));
                }
            }

            if (!isApproved)
            {
                request.Status = RescheduleRequestStatus.Rejected;
                request.RejectionReason = rejectionReason;
                request.RespondedAt = DateTime.UtcNow;
                request.RespondedBy = respondedBy;
                await _rescheduleRequestRepository.SaveChangesAsync();

                // Notify requester — reschedule rejected
                _backgroundService.Enqueue<INotificationUseCase>(
                    uc => uc.CreateAsync(
                        request.RequestedBy,
                        NotificationType.RescheduleRejected,
                        "Reschedule rejected",
                        rejectionReason ?? "Your reschedule request has been rejected.",
                        "/interview?tab=upcoming",
                        requestId));

                var requester = await _userRepository.GetByIdAsync(request.RequestedBy);
                if (requester != null)
                {
                    var placeholders = new Dictionary<string, string>
                    {
                        ["RecipientName"] = requester.FullName,
                        ["Status"] = "Rejected",
                        ["RejectionReason"] = rejectionReason ?? "No reason provided",
                        ["NewTime"] = "-"
                    };

                    _backgroundService.Enqueue<IEmailService>(svc => svc.SendEmailWithTemplateAsync(
                        requester.Email,
                        "RescheduleResponse",
                        placeholders));
                }
            }
        }
    }
}

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
    // TODO: Re-enable when approval flow is implemented — currently reschedule requests are auto-approved
    internal class RespondToRescheduleRequestUseCase : IRespondToRescheduleRequestUseCase
    {
        private readonly ILogger<RespondToRescheduleRequestUseCase> _logger;
        private readonly IRescheduleRequestRepository _rescheduleRequestRepository;
        private readonly IInterviewRoomRepository _interviewRoomRepository;
        private readonly ICoachAvailabilitiesRepository _coachAvailabilitiesRepository;
        private readonly IBookingRequestRepository _bookingRequestRepository;
        private readonly IBackgroundService _backgroundService;
        private readonly IUserRepository _userRepository;
        public RespondToRescheduleRequestUseCase(
            ILogger<RespondToRescheduleRequestUseCase> logger,
            IRescheduleRequestRepository rescheduleRequestRepository,
            IInterviewRoomRepository interviewRoomRepository,
            ICoachAvailabilitiesRepository coachAvailabilitiesRepository,
            IBookingRequestRepository bookingRequestRepository,
            IBackgroundService backgroundService,
            IUserRepository userRepository)
        {
            _logger = logger;
            _rescheduleRequestRepository = rescheduleRequestRepository;
            _interviewRoomRepository = interviewRoomRepository;
            _coachAvailabilitiesRepository = coachAvailabilitiesRepository;
            _bookingRequestRepository = bookingRequestRepository;
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

                var round = await ResolveRoundForRoomAsync(room);
                var newBlocks = await GetBlocksForRangeAsync(
                    room.CoachId ?? throw new ConflictException("Interview room has no coach assigned"),
                    request.ProposedStartTime,
                    request.ProposedEndTime,
                    round.Id);

                request.CurrentAvailabilityId = room.CurrentAvailabilityId;
                request.ProposedAvailabilityId = newBlocks.FirstOrDefault()?.Id;

                // Update room scheduled time based on proposed start time
                room.ScheduledTime = request.ProposedStartTime;
                room.RescheduleAttemptCount++;
                room.CurrentAvailabilityId = newBlocks.FirstOrDefault()?.Id;

                ApplyRoundReschedule(round, newBlocks, request.ProposedStartTime, request.ProposedEndTime);

                _interviewRoomRepository.UpdateAsync(room);
                await _interviewRoomRepository.SaveChangesAsync();

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
                    try
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
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to enqueue approved reschedule response email for request {RequestId}", requestId);
                    }
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
                    try
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
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to enqueue rejected reschedule response email for request {RequestId}", requestId);
                    }
                }
            }
        }

        private async Task<InterviewRound> ResolveRoundForRoomAsync(Domain.Entities.InterviewRoom room)
        {
            if (!room.BookingRequestId.HasValue)
            {
                throw new ConflictException("Interview room is missing booking request");
            }

            var bookingRequest = await _bookingRequestRepository.GetByIdWithDetailsAsync(room.BookingRequestId.Value)
                ?? throw new ConflictException("Booking request not found for this interview room");

            var round = bookingRequest.Rounds.FirstOrDefault(r => r.InterviewRoomId == room.Id)
                ?? (room.RoundNumber.HasValue
                    ? bookingRequest.Rounds.FirstOrDefault(r => r.RoundNumber == room.RoundNumber.Value)
                    : null);

            if (round == null)
            {
                throw new ConflictException("Interview round not found for reschedule");
            }

            return round;
        }

        private async Task<List<CoachAvailability>> GetBlocksForRangeAsync(
            Guid coachId,
            DateTime startTime,
            DateTime endTime,
            Guid roundId)
        {
            var blocks = await _coachAvailabilitiesRepository.GetBlocksInRangeForUpdateAsync(coachId, startTime, endTime);
            if (blocks.Count == 0)
            {
                throw new ConflictException("No availability blocks found for the proposed time");
            }

            EnsureBlocksCoverRange(startTime, endTime, blocks);

            foreach (var block in blocks)
            {
                if (block.Status != CoachAvailabilityStatus.Available && block.InterviewRoundId != roundId)
                {
                    throw new ConflictException("The proposed time includes booked availability blocks");
                }
            }

            return blocks;
        }

        private void ApplyRoundReschedule(
            InterviewRound round,
            List<CoachAvailability> newBlocks,
            DateTime newStartTime,
            DateTime newEndTime)
        {
            var newBlockIds = newBlocks.Select(b => b.Id).ToHashSet();

            foreach (var oldBlock in round.AvailabilityBlocks ?? [])
            {
                if (newBlockIds.Contains(oldBlock.Id))
                {
                    continue;
                }

                oldBlock.Status = CoachAvailabilityStatus.Available;
                oldBlock.InterviewRoundId = null;
                _coachAvailabilitiesRepository.UpdateAsync(oldBlock);
            }

            foreach (var block in newBlocks)
            {
                block.Status = CoachAvailabilityStatus.Booked;
                block.InterviewRoundId = round.Id;
                _coachAvailabilitiesRepository.UpdateAsync(block);
            }

            round.StartTime = newStartTime;
            round.EndTime = newEndTime;
            round.AvailabilityBlocks = newBlocks;
        }

        private static void EnsureBlocksCoverRange(
            DateTime startTime,
            DateTime endTime,
            List<CoachAvailability> blocks)
        {
            var cursor = startTime;
            foreach (var block in blocks.OrderBy(b => b.StartTime))
            {
                if (block.StartTime > cursor)
                {
                    break;
                }

                if (block.EndTime > cursor)
                {
                    cursor = block.EndTime;
                }
            }

            if (cursor < endTime)
            {
                throw new ConflictException("The proposed time is not fully covered by availability blocks");
            }
        }
    }
}

using Intervu.Application.DTOs.BookingRequest;
using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.BookingRequest;
using Intervu.Application.Interfaces.UseCases.Notification;
using Intervu.Application.Services;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Intervu.Application.UseCases.BookingRequest
{
    internal class RescheduleJDBookingRequest : IRescheduleJDBookingRequest
    {
        private static readonly TimeSpan MinGapBetweenRounds = TimeSpan.FromMinutes(15);

        private readonly ILogger<RescheduleJDBookingRequest> _logger;
        private readonly IBookingRequestRepository _bookingRepo;
        private readonly IInterviewRoomRepository _roomRepo;
        private readonly IRescheduleRequestRepository _rescheduleRequestRepo;
        private readonly ICoachAvailabilitiesRepository _availabilityRepo;
        private readonly ITransactionRepository _transactionRepo;
        private readonly IBackgroundService _backgroundService;

        public RescheduleJDBookingRequest(
            ILogger<RescheduleJDBookingRequest> logger,
            IBookingRequestRepository bookingRepo,
            IInterviewRoomRepository roomRepo,
            IRescheduleRequestRepository rescheduleRequestRepo,
            ICoachAvailabilitiesRepository availabilityRepo,
            ITransactionRepository transactionRepo,
            IBackgroundService backgroundService)
        {
            _logger = logger;
            _bookingRepo = bookingRepo;
            _roomRepo = roomRepo;
            _rescheduleRequestRepo = rescheduleRequestRepo;
            _availabilityRepo = availabilityRepo;
            _transactionRepo = transactionRepo;
            _backgroundService = backgroundService;
        }

        public async Task ExecuteAsync(Guid candidateId, Guid bookingRequestId, RescheduleJDBookingRequestDto dto)
        {
            if (dto.Rounds == null || dto.Rounds.Count == 0)
            {
                throw new BadRequestException("At least one round must be selected for reschedule");
            }

            var duplicateRoomIds = dto.Rounds
                .GroupBy(r => r.InterviewRoomId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateRoomIds.Count > 0)
            {
                throw new BadRequestException("Duplicate interview rounds found in reschedule request");
            }

            var bookingRequest = await _bookingRepo.GetByIdWithDetailsAsync(bookingRequestId)
                ?? throw new NotFoundException("Booking request not found");

            if (bookingRequest.Type != BookingRequestType.JDInterview)
            {
                throw new ConflictException("Only JD multi-round bookings support this reschedule endpoint");
            }

            if (bookingRequest.CandidateId != candidateId)
            {
                throw new ForbiddenException("You can only reschedule your own booking");
            }

            if (bookingRequest.Status != BookingRequestStatus.Paid)
            {
                throw new ConflictException("Only paid JD bookings can be rescheduled");
            }

            var bookingRooms = await _roomRepo.GetByBookingRequestIdAsync(bookingRequestId);
            if (bookingRooms.Count == 0)
            {
                throw new ConflictException("No interview rounds found for this booking");
            }

            var roomById = bookingRooms.ToDictionary(r => r.Id);
            var roomIdsInBooking = bookingRooms.Select(r => r.Id).ToHashSet();

            var selectedMappings = dto.Rounds.Select(r =>
            {
                if (!roomById.TryGetValue(r.InterviewRoomId, out var room))
                {
                    throw new BadRequestException($"Interview room {r.InterviewRoomId} does not belong to this booking");
                }

                if (room.RoundNumber == null)
                {
                    throw new ConflictException($"Interview room {room.Id} has no round number");
                }

                if (room.ScheduledTime == null)
                {
                    throw new ConflictException($"Interview room {room.Id} has no scheduled time");
                }

                if (room.Status != InterviewRoomStatus.Scheduled)
                {
                    throw new ConflictException($"Round {room.RoundNumber} is not in scheduled state and cannot be rescheduled");
                }

                return new SelectedRoundMapping(
                    room,
                    EnsureUtc(r.NewStartTime));
            }).ToList();

            foreach (var selected in selectedMappings)
            {
                var room = selected.Room;
                var timeUntilInterview = room.ScheduledTime!.Value - DateTime.UtcNow;

                if (await _rescheduleRequestRepo.HasPendingRequestAsync(room.Id))
                {
                    throw new ConflictException($"Round {room.RoundNumber} already has a pending reschedule request");
                }

                if (!room.IsAvailableForReschedule())
                {
                    if (timeUntilInterview < TimeSpan.FromHours(12))
                    {
                        throw new ConflictException($"Round {room.RoundNumber} cannot be rescheduled within 12 hours");
                    }

                    if (room.RescheduleAttemptCount >= 1)
                    {
                        throw new ConflictException($"Round {room.RoundNumber} has already been rescheduled once");
                    }

                    throw new ConflictException($"Round {room.RoundNumber} is not available for reschedule");
                }

                if (selected.NewStartTime <= DateTime.UtcNow)
                {
                    throw new ConflictException($"Round {room.RoundNumber} must be rescheduled to a future time");
                }

                var currentStart = EnsureUtc(room.ScheduledTime.Value);
                if (selected.NewStartTime == currentStart)
                {
                    throw new ConflictException($"Round {room.RoundNumber} new time must be different from current time");
                }
            }

            var roundByNumber = bookingRequest.Rounds.ToDictionary(r => r.RoundNumber);
            var selectedByRoomId = selectedMappings.ToDictionary(x => x.Room.Id, x => x.NewStartTime);

            var finalTimeline = bookingRooms
                .OrderBy(r => r.RoundNumber)
                .Select(room =>
                {
                    var roundNumber = room.RoundNumber
                        ?? throw new ConflictException($"Interview room {room.Id} has no round number");

                    if (!roundByNumber.TryGetValue(roundNumber, out var round))
                    {
                        throw new ConflictException($"Could not find interview round entity for round {roundNumber}");
                    }

                    var duration = room.DurationMinutes ?? (int)(round.EndTime - round.StartTime).TotalMinutes;
                    if (duration <= 0)
                    {
                        throw new ConflictException($"Round {roundNumber} has invalid duration");
                    }

                    var originalStart = room.ScheduledTime
                        ?? throw new ConflictException($"Round {roundNumber} has no scheduled time");

                    var effectiveStart = selectedByRoomId.TryGetValue(room.Id, out var newStart)
                        ? newStart
                        : EnsureUtc(originalStart);

                    return new RoundTimelineItem(
                        room.Id,
                        roundNumber,
                        duration,
                        EnsureUtc(originalStart),
                        effectiveStart);
                })
                .ToList();

            ValidateRoundSequence(finalTimeline);
            await ValidateCoachAvailability(bookingRequest.CoachId, selectedMappings, finalTimeline);
            await ValidateCandidateConflicts(candidateId, roomIdsInBooking, finalTimeline, selectedByRoomId.Keys.ToHashSet());

            foreach (var selected in selectedMappings)
            {
                var room = selected.Room;
                var roundNumber = room.RoundNumber!.Value;
                var round = roundByNumber[roundNumber];
                var timeline = finalTimeline.First(t => t.RoomId == room.Id);

                room.ScheduledTime = timeline.NewStartTime;
                room.RescheduleAttemptCount += 1;
                _roomRepo.UpdateAsync(room);

                round.StartTime = timeline.NewStartTime;
                round.EndTime = timeline.NewEndTime;
            }

            bookingRequest.UpdatedAt = DateTime.UtcNow;
            _bookingRepo.UpdateAsync(bookingRequest);
            await _bookingRepo.SaveChangesAsync();

            var rescheduledCount = selectedMappings.Count;
            var candidateName = bookingRequest.Candidate?.User?.FullName ?? "Candidate";

            _backgroundService.Enqueue<INotificationUseCase>(
                uc => uc.CreateAsync(
                    candidateId,
                    NotificationType.RescheduleAccepted,
                    "Reschedule successful",
                    rescheduledCount == 1
                        ? "Your interview round has been rescheduled successfully."
                        : $"Your {rescheduledCount} interview rounds have been rescheduled successfully.",
                    "/interview?tab=upcoming",
                    bookingRequestId));

            _backgroundService.Enqueue<INotificationUseCase>(
                uc => uc.CreateAsync(
                    bookingRequest.CoachId,
                    NotificationType.RescheduleAccepted,
                    "Interview schedule updated",
                    rescheduledCount == 1
                        ? $"{candidateName} has rescheduled 1 interview round."
                        : $"{candidateName} has rescheduled {rescheduledCount} interview rounds.",
                    "/interview?tab=upcoming",
                    bookingRequestId));

            _logger.LogInformation(
                "Candidate {CandidateId} rescheduled {Count} round(s) for JD booking {BookingRequestId}",
                candidateId,
                rescheduledCount,
                bookingRequestId);
        }

        private async Task ValidateCoachAvailability(
            Guid coachId,
            List<SelectedRoundMapping> selectedMappings,
            List<RoundTimelineItem> finalTimeline)
        {
            var rawAvailabilities = (await _availabilityRepo
                .GetCoachAvailabilitiesByMonthAsync(coachId, 0, 0))
                .ToList();

            if (rawAvailabilities.Count == 0)
            {
                throw new ConflictException("Coach has no available slots to reschedule");
            }

            var rangeStart = rawAvailabilities.Min(a => a.StartTime);
            var rangeEnd = rawAvailabilities.Max(a => a.EndTime);

            var activeTransactions = await _transactionRepo
                .GetActiveBookingsByCoachAsync(coachId, rangeStart, rangeEnd);

            var activeRounds = await _bookingRepo
                .GetActiveRoundsByCoachAsync(coachId, rangeStart, rangeEnd);

            var selectedRoundIds = selectedMappings
                .Select(x => x.Room.Id)
                .ToHashSet();

            var selectedCurrentIntervals = finalTimeline
                .Where(t => selectedRoundIds.Contains(t.RoomId))
                .Select(t => (Start: t.CurrentStartTime, End: t.CurrentStartTime.AddMinutes(t.DurationMinutes)))
                .ToList();

            var filteredActiveRounds = activeRounds
                .Where(interval => !selectedCurrentIntervals.Any(s =>
                    s.Start == EnsureUtc(interval.Start) && s.End == EnsureUtc(interval.End)))
                .ToList();

            var allBookedIntervals = activeTransactions
                .Where(t => t.BookedStartTime.HasValue && t.BookedDurationMinutes.HasValue)
                .Select(t => (
                    Start: EnsureUtc(t.BookedStartTime!.Value),
                    End: EnsureUtc(t.BookedStartTime!.Value).AddMinutes(t.BookedDurationMinutes!.Value)
                ))
                .Concat(filteredActiveRounds.Select(x => (EnsureUtc(x.Start), EnsureUtc(x.End))))
                .ToList();

            var freeSlots = AvailabilityCalculatorService.CalculateFreeSlots(rawAvailabilities, allBookedIntervals);
            var selectedRoomIds = selectedMappings.Select(x => x.Room.Id).ToHashSet();

            foreach (var round in finalTimeline.Where(t => selectedRoomIds.Contains(t.RoomId)))
            {
                var fits = freeSlots.Any(slot =>
                    round.NewStartTime >= slot.Start && round.NewEndTime <= slot.End);

                if (!fits)
                {
                    throw new ConflictException(
                        $"Round {round.RoundNumber} does not fit in coach available slots at the proposed time");
                }
            }
        }

        private async Task ValidateCandidateConflicts(
            Guid candidateId,
            HashSet<Guid> roomIdsInBooking,
            List<RoundTimelineItem> finalTimeline,
            HashSet<Guid> selectedRoomIds)
        {
            foreach (var round in finalTimeline.Where(t => selectedRoomIds.Contains(t.RoomId)))
            {
                var conflicts = await _roomRepo.GetConflictingRoomsAsync(candidateId, round.NewStartTime, round.NewEndTime);
                if (conflicts.Any(c => !roomIdsInBooking.Contains(c.Id)))
                {
                    throw new ConflictException(
                        $"Round {round.RoundNumber} conflicts with one of your existing interview sessions");
                }
            }
        }

        private static void ValidateRoundSequence(List<RoundTimelineItem> finalTimeline)
        {
            for (int i = 1; i < finalTimeline.Count; i++)
            {
                var prev = finalTimeline[i - 1];
                var current = finalTimeline[i];

                if (current.NewStartTime < prev.NewEndTime.Add(MinGapBetweenRounds))
                {
                    throw new ConflictException(
                        $"Round {current.RoundNumber} must start at least 15 minutes after round {prev.RoundNumber} ends");
                }
            }
        }

        private static DateTime EnsureUtc(DateTime value)
        {
            if (value.Kind == DateTimeKind.Utc)
            {
                return value;
            }

            if (value.Kind == DateTimeKind.Unspecified)
            {
                return DateTime.SpecifyKind(value, DateTimeKind.Utc);
            }

            return value.ToUniversalTime();
        }

        private sealed record RoundTimelineItem(
            Guid RoomId,
            int RoundNumber,
            int DurationMinutes,
            DateTime CurrentStartTime,
            DateTime NewStartTime)
        {
            public DateTime NewEndTime => NewStartTime.AddMinutes(DurationMinutes);
        }

        private sealed record SelectedRoundMapping(
            Domain.Entities.InterviewRoom Room,
            DateTime NewStartTime);
    }
}

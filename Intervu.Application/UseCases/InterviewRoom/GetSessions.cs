using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.InterviewRoom;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.InterviewRoom
{
    internal class GetSessions : IGetSessions
    {
        private readonly IInterviewRoomRepository _repo;
        private readonly IRescheduleRequestRepository _rescheduleRepo;
        private readonly IFeedbackRepository _feedbackRepo;

        public GetSessions(
            IInterviewRoomRepository repo,
            IRescheduleRequestRepository rescheduleRepo,
            IFeedbackRepository feedbackRepo)
        {
            _repo = repo;
            _rescheduleRepo = rescheduleRepo;
            _feedbackRepo = feedbackRepo;
        }

        public async Task<PagedSessionsResultDto> ExecuteAsync(
            UserRole role,
            Guid userId,
            GetSessionsRequestDto request)
        {
            var tuples = role == UserRole.Candidate
                ? await _repo.GetListWithNamesByCandidateIdAsync(userId)
                : await _repo.GetListWithNamesByCoachIdAsync(userId);

            var allRooms = tuples
                .Select(t => new RoomBundle(
                    t.Item1,
                    t.Item2, t.Item3, t.Item4,
                    t.Item5, t.Item6, t.Item7))
                .ToList();

            if (allRooms.Count == 0)
            {
                return new PagedSessionsResultDto(
                    new PagedResult<SessionDto>(new List<SessionDto>(), 0, request.PageSize, request.Page),
                    new SessionStatsDto(),
                    null);
            }

            var allRoomIds = allRooms.Select(r => r.Room.Id).ToList();
            var pendingRescheduleSet = await _rescheduleRepo.GetPendingRequestRoomIdsAsync(allRoomIds);
            var ratingByRoomId = await _feedbackRepo.GetRatingsByInterviewRoomIdsAsync(allRoomIds);

            var sessions = allRooms
                .GroupBy(r => r.Room.BookingRequestId ?? r.Room.Id)
                .Select(g => BuildSession(g.Key, g.ToList()))
                .ToList();

            var stats = ComputeStats(role, sessions, ratingByRoomId);

            SessionDto? pendingDto = null;
            if (role == UserRole.Coach)
            {
                var pendingRoom = allRooms.FirstOrDefault(r =>
                    r.Room.Status == InterviewRoomStatus.Completed && !r.Room.IsEvaluationCompleted);

                if (pendingRoom != null)
                {
                    var hostSession = sessions.First(s => s.Rounds.Any(rn => rn.Room.Id == pendingRoom.Room.Id));
                    pendingDto = ProjectSession(hostSession, pendingRescheduleSet, ratingByRoomId, overrideActive: pendingRoom);
                }
            }

            IEnumerable<Session> filtered = sessions;
            if (request.Statuses != null && request.Statuses.Count > 0)
            {
                var statusSet = request.Statuses.ToHashSet();
                filtered = filtered.Where(s => statusSet.Contains(s.Active.Room.Status));
            }

            var ordered = filtered
                .OrderByDescending(s => s.Active.Room.ScheduledTime ?? DateTime.MinValue)
                .ToList();

            var totalItems = ordered.Count;

            var pagedSessions = ordered
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var items = pagedSessions
                .Select(s => ProjectSession(s, pendingRescheduleSet, ratingByRoomId, overrideActive: null))
                .ToList();

            return new PagedSessionsResultDto(
                new PagedResult<SessionDto>(items, totalItems, request.PageSize, request.Page),
                stats,
                pendingDto);
        }

        // ----- Helpers -----

        private sealed class RoomBundle
        {
            public Domain.Entities.InterviewRoom Room { get; }
            public string? CandidateName { get; }
            public string? CandidateProfilePicture { get; }
            public string? CandidateSlugProfileUrl { get; }
            public string? CoachName { get; }
            public string? CoachProfilePicture { get; }
            public string? CoachSlugProfileUrl { get; }

            public RoomBundle(
                Domain.Entities.InterviewRoom room,
                string? candidateName,
                string? candidateProfilePicture,
                string? candidateSlugProfileUrl,
                string? coachName,
                string? coachProfilePicture,
                string? coachSlugProfileUrl)
            {
                Room = room;
                CandidateName = candidateName;
                CandidateProfilePicture = candidateProfilePicture;
                CandidateSlugProfileUrl = candidateSlugProfileUrl;
                CoachName = coachName;
                CoachProfilePicture = coachProfilePicture;
                CoachSlugProfileUrl = coachSlugProfileUrl;
            }
        }

        private sealed class Session
        {
            public Guid SessionId { get; }
            public List<RoomBundle> Rounds { get; }
            public RoomBundle Active { get; }
            public int CurrentRoundIndex { get; }

            public Session(Guid sessionId, List<RoomBundle> rounds, RoomBundle active, int currentRoundIndex)
            {
                SessionId = sessionId;
                Rounds = rounds;
                Active = active;
                CurrentRoundIndex = currentRoundIndex;
            }
        }

        private static Session BuildSession(Guid sessionId, List<RoomBundle> group)
        {
            var sorted = group
                .OrderBy(r => r.Room.ScheduledTime ?? DateTime.MinValue)
                .ThenBy(r => r.Room.RoundNumber ?? 0)
                .ToList();

            int activeIdx = sorted.FindIndex(r => r.Room.Status == InterviewRoomStatus.Ongoing);
            if (activeIdx < 0) activeIdx = sorted.FindIndex(r => r.Room.Status == InterviewRoomStatus.Scheduled);
            if (activeIdx < 0) activeIdx = sorted.Count - 1;

            return new Session(sessionId, sorted, sorted[activeIdx], activeIdx);
        }

        private static SessionStatsDto ComputeStats(
            UserRole role,
            List<Session> sessions,
            IReadOnlyDictionary<Guid, double?> ratingByRoomId)
        {
            int upcoming = 0;
            int completed = 0;
            var scores = new List<double>();
            DateTime? earliestFuture = null;
            var now = DateTime.UtcNow;

            foreach (var s in sessions)
            {
                var active = s.Active.Room;
                switch (active.Status)
                {
                    case InterviewRoomStatus.Scheduled:
                    case InterviewRoomStatus.Ongoing:
                        upcoming++;
                        if (active.Status == InterviewRoomStatus.Scheduled
                            && active.ScheduledTime.HasValue
                            && active.ScheduledTime.Value > now
                            && (earliestFuture == null || active.ScheduledTime.Value < earliestFuture.Value))
                        {
                            earliestFuture = active.ScheduledTime.Value;
                        }
                        break;

                    case InterviewRoomStatus.Completed:
                        completed++;
                        if (role == UserRole.Candidate)
                        {
                            if (active.EvaluationResults?.Any() == true)
                            {
                                scores.Add(active.EvaluationResults.Average(x => x.Score));
                            }
                        }
                        else
                        {
                            if (ratingByRoomId.TryGetValue(active.Id, out var rating) && rating.HasValue)
                            {
                                scores.Add(rating.Value);
                            }
                        }
                        break;
                }
            }

            return new SessionStatsDto
            {
                Upcoming = upcoming,
                Completed = completed,
                AvgScore = scores.Count > 0 ? Math.Round(scores.Average(), 1) : (double?)null,
                NextSessionInMs = earliestFuture.HasValue
                    ? (long?)Math.Max(0, (long)(earliestFuture.Value - now).TotalMilliseconds)
                    : null,
            };
        }

        private static SessionDto ProjectSession(
            Session session,
            HashSet<Guid> pendingRescheduleSet,
            IReadOnlyDictionary<Guid, double?> ratingByRoomId,
            RoomBundle? overrideActive)
        {
            var active = overrideActive ?? session.Active;
            var activeIdx = overrideActive != null
                ? session.Rounds.FindIndex(r => r.Room.Id == overrideActive.Room.Id)
                : session.CurrentRoundIndex;

            var rounds = session.Rounds
                .Select(r => ProjectRoom(r, pendingRescheduleSet, ratingByRoomId))
                .ToList();

            var activeDto = ProjectRoom(active, pendingRescheduleSet, ratingByRoomId);

            return new SessionDto
            {
                Id = activeDto.Id,
                CandidateId = activeDto.CandidateId,
                CandidateName = activeDto.CandidateName,
                CandidateProfilePicture = activeDto.CandidateProfilePicture,
                CandidateSlugProfileUrl = activeDto.CandidateSlugProfileUrl,
                CoachId = activeDto.CoachId,
                CoachName = activeDto.CoachName,
                CoachProfilePicture = activeDto.CoachProfilePicture,
                CoachSlugProfileUrl = activeDto.CoachSlugProfileUrl,
                ScheduledTime = activeDto.ScheduledTime,
                DurationMinutes = activeDto.DurationMinutes,
                VideoCallRoomUrl = activeDto.VideoCallRoomUrl,
                CurrentLanguage = activeDto.CurrentLanguage,
                LanguageCodes = activeDto.LanguageCodes,
                ProblemDescription = activeDto.ProblemDescription,
                ProblemShortName = activeDto.ProblemShortName,
                TestCases = activeDto.TestCases,
                Status = activeDto.Status,
                IsEvaluationCompleted = activeDto.IsEvaluationCompleted,
                Score = activeDto.Score,
                Rating = activeDto.Rating,
                RescheduleAttemptCount = activeDto.RescheduleAttemptCount,
                Type = activeDto.Type,
                BookingRequestId = activeDto.BookingRequestId,
                JobDescriptionUrl = activeDto.JobDescriptionUrl,
                CVUrl = activeDto.CVUrl,
                InterviewTypeName = activeDto.InterviewTypeName,
                AimLevel = activeDto.AimLevel,
                RoundNumber = activeDto.RoundNumber,
                HasPendingReschedule = activeDto.HasPendingReschedule,
                CanReschedule = activeDto.CanReschedule,
                CanCancel = activeDto.CanCancel,
                SessionId = session.SessionId,
                CurrentRound = activeIdx + 1,
                TotalRounds = session.Rounds.Count,
                Rounds = rounds,
            };
        }

        private static InterviewRoomDto ProjectRoom(
            RoomBundle bundle,
            HashSet<Guid> pendingRescheduleSet,
            IReadOnlyDictionary<Guid, double?> ratingByRoomId)
        {
            var room = bundle.Room;
            var hasPendingReschedule = pendingRescheduleSet.Contains(room.Id);
            var canReschedule = room.IsAvailableForReschedule() && !hasPendingReschedule;
            var canCancel = room.IsAvailableForCancel();
            ratingByRoomId.TryGetValue(room.Id, out var rating);

            return new InterviewRoomDto
            {
                Id = room.Id,
                CandidateId = room.CandidateId,
                CandidateName = bundle.CandidateName,
                CandidateProfilePicture = bundle.CandidateProfilePicture,
                CandidateSlugProfileUrl = bundle.CandidateSlugProfileUrl,
                CoachId = room.CoachId,
                CoachName = bundle.CoachName,
                CoachProfilePicture = bundle.CoachProfilePicture,
                CoachSlugProfileUrl = bundle.CoachSlugProfileUrl,
                ScheduledTime = room.ScheduledTime,
                DurationMinutes = room.DurationMinutes,
                VideoCallRoomUrl = room.VideoCallRoomUrl,
                CurrentLanguage = room.CurrentLanguage,
                ProblemDescription = room.ProblemDescription,
                ProblemShortName = room.ProblemShortName,
                Status = room.Status,
                IsEvaluationCompleted = room.IsEvaluationCompleted,
                Score = room.EvaluationResults?.Any() == true
                    ? Math.Round(room.EvaluationResults.Average(x => x.Score), 1)
                    : (double?)null,
                Rating = rating,
                RescheduleAttemptCount = room.RescheduleAttemptCount,
                BookingRequestId = room.BookingRequestId,
                JobDescriptionUrl = room.BookingRequest?.JobDescriptionUrl,
                CVUrl = room.BookingRequest?.CVUrl,
                InterviewTypeName = room.CoachInterviewService?.InterviewType?.Name,
                AimLevel = room.AimLevel,
                RoundNumber = room.RoundNumber,
                HasPendingReschedule = hasPendingReschedule,
                CanReschedule = canReschedule,
                CanCancel = canCancel,
                Type = room.Type,
            };
        }
    }
}

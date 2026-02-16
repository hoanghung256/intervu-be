using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.InterviewRoom;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.InterviewRoom
{
    internal class GetRoomHistory : IGetRoomHistory
    {
        private readonly IInterviewRoomRepository _repo;
        private readonly IRescheduleRequestRepository _rescheduleRepo;

        public GetRoomHistory(IInterviewRoomRepository repo, IRescheduleRequestRepository rescheduleRepo)
        {
            _repo = repo;
            _rescheduleRepo = rescheduleRepo;
        }

        public async Task<IEnumerable<Domain.Entities.InterviewRoom>> ExecuteAsync(UserRole role, Guid userId)
        {
            if (role == UserRole.Candidate)
            {
                return await _repo.GetListByCandidateId(userId);
            }
            else
            {
                return await _repo.GetListByCoachId(userId);
            }
        }

        public async Task<IEnumerable<Domain.Entities.InterviewRoom>> ExecuteAsync()
        {
            return await _repo.GetList();
        }

        public async Task<PagedResult<InterviewRoomDto>> ExecuteWithPaginationAsync(
            UserRole role, 
            Guid userId, 
            GetInterviewRoomsRequestDto request)
        {
            // Get all rooms for the user with names
            IEnumerable<(Domain.Entities.InterviewRoom Room, string? CandidateName, string? CoachName)> roomsWithNames;
            if (role == UserRole.Candidate)
            {
                roomsWithNames = await _repo.GetListWithNamesByCandidateIdAsync(userId);
            }
            else
            {
                roomsWithNames = await _repo.GetListWithNamesByCoachIdAsync(userId);
            }

            var roomList = roomsWithNames.ToList();

            // Apply filters
            if (request.Statuses != null && request.Statuses.Any())
            {
                roomList = roomList.Where(r => request.Statuses.Contains(r.Room.Status)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(request.SearchQuery))
            {
                var query = request.SearchQuery.ToLower();
                roomList = roomList.Where(r =>
                    (r.Room.ProblemShortName?.ToLower().Contains(query) ?? false) ||
                    (r.Room.ProblemDescription?.ToLower().Contains(query) ?? false) ||
                    (r.CandidateName?.ToLower().Contains(query) ?? false) ||
                    (r.CoachName?.ToLower().Contains(query) ?? false)
                ).ToList();
            }

            // Get total count before pagination
            var totalItems = roomList.Count;

            // Apply pagination
            var pagedRooms = roomList
                .OrderByDescending(r => r.Room.ScheduledTime)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // Map to DTOs with reschedule status
            var dtos = new List<InterviewRoomDto>();
            foreach (var item in pagedRooms)
            {
                var room = item.Room;
                var hasPendingReschedule = await _rescheduleRepo.HasPendingRequestAsync(room.Id);
                var canReschedule = room.IsAvailableForReschedule() && !hasPendingReschedule;

                dtos.Add(new InterviewRoomDto
                {
                    Id = room.Id,
                    CandidateId = room.CandidateId,
                    CandidateName = item.CandidateName,
                    CoachId = room.CoachId,
                    CoachName = item.CoachName,
                    ScheduledTime = room.ScheduledTime,
                    DurationMinutes = room.DurationMinutes,
                    VideoCallRoomUrl = room.VideoCallRoomUrl,
                    CurrentLanguage = room.CurrentLanguage,
                    ProblemDescription = room.ProblemDescription,
                    ProblemShortName = room.ProblemShortName,
                    Status = room.Status,
                    RescheduleAttemptCount = room.RescheduleAttemptCount,
                    HasPendingReschedule = hasPendingReschedule,
                    CanReschedule = canReschedule
                });
            }

            return new PagedResult<InterviewRoomDto>(dtos, totalItems, request.PageSize, request.Page);
        }
    }
}

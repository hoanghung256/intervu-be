using Intervu.Application.DTOs.CoachDashboard;
using Intervu.Application.Interfaces.UseCases.CoachDashboard;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.CoachDashboard
{
    public class GetCoachUpcomingSessions : IGetCoachUpcomingSessions
    {
        private readonly IInterviewRoomRepository _roomRepo;

        public GetCoachUpcomingSessions(IInterviewRoomRepository roomRepo)
        {
            _roomRepo = roomRepo;
        }

        public async Task<List<CoachUpcomingSessionDto>> ExecuteAsync(Guid coachId)
        {
            var rooms = await _roomRepo.GetUpcomingByCoachIdAsync(coachId, 10);

            return rooms.Select(r =>
            {
                var status = MapStatus(r.BookingStatus);
                var roomIdShort = r.Room.Id.ToString()[..4].ToUpper();

                return new CoachUpcomingSessionDto
                {
                    InterviewRoomId = r.Room.Id,
                    CandidateName = r.CandidateName ?? "Unknown",
                    CandidateProfilePicture = r.CandidateProfilePicture,
                    RoomIdDisplay = $"ROOM-{roomIdShort}",
                    ScheduledTime = r.Room.ScheduledTime,
                    Status = status
                };
            }).ToList();
        }

        private static string MapStatus(string? bookingStatus)
        {
            if (bookingStatus == null) return "Confirmed";

            return bookingStatus switch
            {
                //nameof(BookingRequestStatus.Paid) => "Confirmed",
                nameof(BookingRequestStatus.Accepted) => "Upcoming",
                nameof(BookingRequestStatus.Pending) => "Pending Payment",
                _ => "Upcoming"
            };
        }
    }
}

using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Application.Services;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.InterviewRoom
{
    public class CreateInterviewRoom : ICreateInterviewRoom
    {
        private readonly IInterviewRoomRepository _interviewRoomRepo;
        private readonly InterviewRoomCache _cache;

        public CreateInterviewRoom(IInterviewRoomRepository interviewRoomRepo, InterviewRoomCache cache)
        {
            _interviewRoomRepo = interviewRoomRepo;
            _cache = cache;
        }

        public async Task<Guid> ExecuteAsync(Guid candidateId)
        {
            Domain.Entities.InterviewRoom room = new()
            {
                CandidateId = candidateId,
            };
            await _interviewRoomRepo.AddAsync(room);
            await _interviewRoomRepo.SaveChangesAsync();

            //Notify SQL Changes
            _cache.Add(room);
            return room.Id;
        }

        public async Task<Guid> ExecuteAsync(Guid candidateId, Guid coachId, Guid availabilityId, DateTime startTime)
        {
            // TODO: interveweeId and interviewerId are valid and exists
            Domain.Entities.InterviewRoom room = new()
            {
                CandidateId = candidateId,
                CoachId = coachId,
                ScheduledTime = startTime,
                Status = Domain.Entities.Constants.InterviewRoomStatus.Scheduled,
                DurationMinutes = 60,
                CurrentAvailabilityId = availabilityId
            };
            await _interviewRoomRepo.AddAsync(room);
            await _interviewRoomRepo.SaveChangesAsync();

            //Notify SQL Changes
            _cache.Add(room);
            return room.Id;
        }
    }
}

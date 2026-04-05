using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Application.Services;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Intervu.Application.UseCases.InterviewRoom
{
    public class CreateInterviewRoom : ICreateInterviewRoom
    {
        private readonly IInterviewRoomRepository _interviewRoomRepo;
        private readonly InterviewRoomCache _cache;
        private readonly ILogger<CreateInterviewRoom> _logger;
        private readonly IScheduleInterviewReminders _scheduleReminders;

        public CreateInterviewRoom(
            IInterviewRoomRepository interviewRoomRepo,
            InterviewRoomCache cache,
            ILogger<CreateInterviewRoom> logger,
            IScheduleInterviewReminders scheduleReminders)
        {
            _interviewRoomRepo = interviewRoomRepo;
            _cache = cache;
            _logger = logger;
            _scheduleReminders = scheduleReminders;
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

        public async Task<Guid> ExecuteAsync(Guid candidateId, Guid coachId, Guid availabilityId, DateTime startTime, Guid transactionId, int duration)
        {
            // TODO: interveweeId and interviewerId are valid and exists
            Domain.Entities.InterviewRoom room = new()
            {
                CandidateId = candidateId,
                CoachId = coachId,
                ScheduledTime = startTime,
                Status = Domain.Entities.Constants.InterviewRoomStatus.Scheduled,
                DurationMinutes = duration,
                CurrentAvailabilityId = availabilityId,
                TransactionId = transactionId
            };
            await _interviewRoomRepo.AddAsync(room);
            await _interviewRoomRepo.SaveChangesAsync();

            _logger.LogInformation("Created room");

            // Schedule reminder notifications at 1 day, 12h, 1h, and 5min before
            _scheduleReminders.Schedule(room.Id, startTime);

            //Notify SQL Changes
            _cache.Add(room);
            return room.Id;
        }

        public async Task ExecuteAsync(Domain.Entities.InterviewRoom room)
        {
            await _interviewRoomRepo.AddAsync(room);
            await _interviewRoomRepo.SaveChangesAsync();

            if (room.ScheduledTime.HasValue)
            {
                // Schedule after persistence so room Id is available.
                _scheduleReminders.Schedule(room.Id, room.ScheduledTime.Value);
            }

            _cache.Add(room);
        }
    }
}

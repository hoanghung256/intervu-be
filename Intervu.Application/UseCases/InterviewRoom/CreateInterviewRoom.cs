using Intervu.Application.Interfaces.Repositories;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;

namespace Intervu.Application.UseCases.InterviewRoom
{
    public class CreateInterviewRoom : ICreateInterviewRoom
    {
        private readonly IInterviewRoomRepository _interviewRoomRepo;

        public CreateInterviewRoom(IInterviewRoomRepository interviewRoomRepo) 
        {
            _interviewRoomRepo = interviewRoomRepo;
        }

        public async Task<int> ExecuteAsync(int interveweeId)
        {
            Domain.Entities.InterviewRoom room = new ()
            {
                StudentId = interveweeId,
            };
            await _interviewRoomRepo.AddAsync(room);
            await _interviewRoomRepo.SaveChangesAsync();
            
            return room.Id;
        }

        public async Task<int> ExecuteAsync(int interveweeId, int interviewerId)
        {
            // Check if interveweeId and interviewerId are valid and exists

            Domain.Entities.InterviewRoom room = new()
            {
                StudentId = interveweeId,
                InterviewerId = interviewerId
            };
            await _interviewRoomRepo.AddAsync(room);
            await _interviewRoomRepo.SaveChangesAsync();

            return room.Id;
        }
    }
}

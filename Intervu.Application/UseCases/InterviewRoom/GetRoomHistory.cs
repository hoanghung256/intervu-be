using Intervu.Application.Interfaces.Repositories;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.UseCases.InterviewRoom
{
    internal class GetRoomHistory : IGetRoomHistory
    {
        private readonly IInterviewRoomRepository _repo;

        public GetRoomHistory(IInterviewRoomRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<Domain.Entities.InterviewRoom>> ExecuteAsync(UserRole role, int userId)
        {
            if (role == UserRole.Interviewee)
            {
                return await _repo.GetListByIntervieweeId(userId);
            }
            else
            {
                return await _repo.GetListByInterviewerId(userId);
            }
        }

        public async Task<IEnumerable<Domain.Entities.InterviewRoom>> ExecuteAsync()
        {
            return await _repo.GetList();
        }
    }
}

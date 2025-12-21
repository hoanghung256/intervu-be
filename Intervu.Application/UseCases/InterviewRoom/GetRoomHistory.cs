using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.InterviewRoom
{
    internal class GetRoomHistory : IGetRoomHistory
    {
        private readonly IInterviewRoomRepository _repo;

        public GetRoomHistory(IInterviewRoomRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<Domain.Entities.InterviewRoom>> ExecuteAsync(UserRole role, Guid userId)
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

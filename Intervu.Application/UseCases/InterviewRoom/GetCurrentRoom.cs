using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.InterviewRoom
{
    public class GetCurrentRoom : IGetCurrentRoom
    {
        private readonly IInterviewRoomRepository _repo;

        public GetCurrentRoom(IInterviewRoomRepository repo)
        {
            _repo = repo;
        }

        public async Task<Domain.Entities.InterviewRoom> ExecuteAsync(Guid roomId)
        {
            return await _repo.GetByIdAsync(roomId);
        }
    }
}

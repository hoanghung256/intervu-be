using Intervu.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.InterviewRoom.InterviewRoom
{
    public class UpdateRoom : IUpdateRoom
    {
        private readonly IInterviewRoomRepository _repo;

        public UpdateRoom(IInterviewRoomRepository repo)
        {
            _repo = repo;
        }

        public async Task ExecuteAsync(Domain.Entities.InterviewRoom interviewRoom)
        {
            _repo.UpdateAsync(interviewRoom);
            await _repo.SaveChangesAsync();
        }
    }
}

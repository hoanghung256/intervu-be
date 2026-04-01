using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Application.Services;
using Intervu.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.InterviewRoom
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

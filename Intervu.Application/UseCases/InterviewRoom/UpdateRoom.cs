using Intervu.Application.Interfaces.Repositories;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Application.Services;
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
        private readonly RoomManagerService _cache;

        public UpdateRoom(IInterviewRoomRepository repo, RoomManagerService cache)
        {
            _repo = repo;
            _cache = cache;
        }

        public async Task ExecuteAsync(Domain.Entities.InterviewRoom interviewRoom)
        {
            _repo.UpdateAsync(interviewRoom);
            await _repo.SaveChangesAsync();

            //Notify SQL Changes
            _cache.Update(interviewRoom);
        }
    }
}

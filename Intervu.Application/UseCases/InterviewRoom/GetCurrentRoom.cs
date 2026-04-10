using AutoMapper;
using Intervu.Application.DTOs.InterviewRoom;
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
        private readonly IMapper _mapper;

        public GetCurrentRoom(IInterviewRoomRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<InterviewRoomDto?> ExecuteAsync(Guid roomId)
        {
            var room = await _repo.GetByIdWithDetailsAsync(roomId);
            if (room == null)
            {
                return null;
            }

            return _mapper.Map<InterviewRoomDto>(room);
        }
    }
}

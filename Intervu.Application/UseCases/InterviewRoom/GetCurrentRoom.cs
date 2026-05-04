using AutoMapper;
using Intervu.Application.DTOs.InterviewRoom;
using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Domain.Repositories;

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

        public async Task<InterviewRoomDto?> ExecuteAsync(Guid roomId, Guid userId)
        {
            var room = await _repo.GetByIdWithDetailsAsync(roomId);
            if (room == null)
            {
                return null;
            }

            if (room.CandidateId != userId && room.CoachId != userId)
            {
                throw new ForbiddenException("You are not authorized to view this interview room");
            }

            return _mapper.Map<InterviewRoomDto>(room);
        }
    }
}

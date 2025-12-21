using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class InterviewRoomRepository(IntervuPostgreDbContext context) : RepositoryBase<InterviewRoom>(context), IInterviewRoomRepository
    {
        public async Task<IEnumerable<InterviewRoom>> GetListByIntervieweeId(Guid intervieweeId)
        {
            return await _context.InterviewRooms.Where(r => r.StudentId == intervieweeId).ToListAsync();
        }

        public async Task<IEnumerable<InterviewRoom>> GetListByInterviewerId(Guid interviewerId)
        {
            return await _context.InterviewRooms.Where(r => r.InterviewerId == interviewerId).ToListAsync();
        }

        public async Task<IEnumerable<InterviewRoom>> GetList()
        {
            return await _context.InterviewRooms.ToListAsync();
        }
    }
}

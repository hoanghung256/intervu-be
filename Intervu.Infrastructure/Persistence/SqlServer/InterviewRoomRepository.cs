using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.SqlServer.DataContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Infrastructure.Persistence.SqlServer
{
    public class InterviewRoomRepository : RepositoryBase<InterviewRoom>, IInterviewRoomRepository
    {
        private readonly IntervuDbContext _context;

        public InterviewRoomRepository(IntervuDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<InterviewRoom>> GetListByCandidateId(Guid candidateId)
        {
            return await _context.InterviewRooms.Where(r => r.CandidateId == candidateId).ToListAsync();
        }

        public async Task<IEnumerable<InterviewRoom>> GetListByCoachId(Guid coachId)
        {
            return await _context.InterviewRooms.Where(r => r.CoachId == coachId).ToListAsync();
        }

        public async Task<IEnumerable<InterviewRoom>> GetList()
        {
            return await _context.InterviewRooms.ToListAsync();
        }

        public Task<IEnumerable<InterviewRoom>> GetConflictingRoomsAsync(Guid userId, DateTime startTime, DateTime endTime)
        {
            throw new NotImplementedException();
        }
    }
}

using Intervu.Application.Interfaces.Repositories;
using Intervu.Domain.Entities;
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

        public async Task<IEnumerable<InterviewRoom>> GetListByIntervieweeId(int intervieweeId)
        {
            return await _context.InterviewRooms.Where(r => r.StudentId == intervieweeId).ToListAsync();
        }

        public async Task<IEnumerable<InterviewRoom>> GetListByInterviewerId(int interviewerId)
        {
            return await _context.InterviewRooms.Where(r => r.InterviewerId == interviewerId).ToListAsync();
        }

        public async Task<IEnumerable<InterviewRoom>> GetList()
        {
            return await _context.InterviewRooms.ToListAsync();
        }
    }
}

using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class GeneratedQuestionRepository(IntervuPostgreDbContext context)
        : RepositoryBase<GeneratedQuestion>(context), IGeneratedQuestionRepository
    {
        public async Task<List<GeneratedQuestion>> GetByInterviewRoomIdAsync(Guid interviewRoomId)
        {
            return await _context.GeneratedQuestions
                .Where(q => q.InterviewRoomId == interviewRoomId)
                .OrderByDescending(q => q.Status)
                .ThenBy(q => q.Title)
                .ToListAsync();
        }

        public async Task<List<GeneratedQuestion>> GetByInterviewRoomIdAsync(Guid interviewRoomId, GeneratedQuestionStatus status)
        {
            return await _context.GeneratedQuestions
                .Where(q => q.InterviewRoomId == interviewRoomId && q.Status == status)
                .OrderBy(q => q.Title)
                .ToListAsync();
        }
    }
}

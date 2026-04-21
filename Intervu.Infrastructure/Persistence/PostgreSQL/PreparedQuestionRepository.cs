using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class PreparedQuestionRepository(IntervuPostgreDbContext context)
        : RepositoryBase<PreparedQuestion>(context), IPreparedQuestionRepository
    {
        public async Task<List<PreparedQuestion>> GetByInterviewRoomIdAsync(Guid interviewRoomId)
        {
            return await _context.PreparedQuestions
                .Where(q => q.InterviewRoomId == interviewRoomId)
                .OrderBy(q => q.SortOrder)
                .ThenBy(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetMaxSortOrderAsync(Guid interviewRoomId)
        {
            var hasAny = await _context.PreparedQuestions
                .AnyAsync(q => q.InterviewRoomId == interviewRoomId);

            if (!hasAny)
            {
                return -1;
            }

            return await _context.PreparedQuestions
                .Where(q => q.InterviewRoomId == interviewRoomId)
                .MaxAsync(q => q.SortOrder);
        }

        public async Task<PreparedQuestion?> FindByRoomAndBankQuestionAsync(Guid interviewRoomId, Guid bankQuestionId)
        {
            return await _context.PreparedQuestions
                .FirstOrDefaultAsync(q => q.InterviewRoomId == interviewRoomId
                    && q.SourceBankQuestionId == bankQuestionId);
        }

        public async Task<List<PreparedQuestion>> GetByIdsAsync(Guid interviewRoomId, IEnumerable<Guid> ids)
        {
            var idSet = ids?.ToHashSet() ?? new HashSet<Guid>();
            if (idSet.Count == 0)
            {
                return new List<PreparedQuestion>();
            }

            return await _context.PreparedQuestions
                .Where(q => q.InterviewRoomId == interviewRoomId && idSet.Contains(q.Id))
                .ToListAsync();
        }
    }
}

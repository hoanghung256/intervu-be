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
    public class InterviewExperienceRepository : RepositoryBase<InterviewExperience>, IInterviewExperienceRepository
    {
        public InterviewExperienceRepository(IntervuPostgreDbContext context) : base(context)
        {
        }

        public async Task<(List<InterviewExperience> Items, int TotalCount)> GetPagedAsync(
            string? searchTerm, string? role, ExperienceLevel? level, string? lastRoundCompleted, int page, int pageSize)
        {
            var query = _context.InterviewExperiences
                .Include(e => e.Questions)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(e =>
                    e.CompanyName.Contains(searchTerm) ||
                    e.Role.Contains(searchTerm) ||
                    e.InterviewProcess.Contains(searchTerm));

            if (!string.IsNullOrWhiteSpace(role))
                query = query.Where(e => e.Role == role);

            if (level.HasValue)
                query = query.Where(e => e.Level == level);

            if (!string.IsNullOrWhiteSpace(lastRoundCompleted))
                query = query.Where(e => e.LastRoundCompleted == lastRoundCompleted);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(e => e.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<InterviewExperience?> GetDetailAsync(Guid id)
        {
            return await _context.InterviewExperiences
                .Include(e => e.Questions)
                .FirstOrDefaultAsync(e => e.Id == id);
        }
    }
}

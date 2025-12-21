using Azure.Core;
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
    public class FeedbackRepository : RepositoryBase<Feedback>, IFeedbackRepository
    {
        public FeedbackRepository(IntervuDbContext context) : base(context)
        {
        }

        public async Task<(IReadOnlyList<Feedback> Items, int TotalCount)> GetPagedFeedbacksAsync(int page, int pageSize)
        {
            var query = _context.Feedbacks.AsQueryable();

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalItems);
        }
        public async Task CreateFeedbackAsync(Feedback feedback)
        {
            await AddAsync(feedback);
            await SaveChangesAsync();
        }

        public async Task<Feedback?> GetFeedbackByIdAsync(Guid id)
        {
            return await GetByIdAsync(id);
        }


        public async Task<(IReadOnlyList<Feedback> Items, int TotalCount)> GetFeedbacksByStudentIdAsync(Guid studentId, int page, int pageSize)
        {
            var query = _context.Feedbacks.Where(f => f.StudentId == studentId).AsQueryable();

            var totalItems = await query.CountAsync();

            var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
                .ToListAsync();

            return (items, totalItems);
        }

        public async Task<int> GetTotalFeedbacksCountAsync()
        {
            return await _context.Feedbacks.CountAsync();
        }

        public async Task<double> GetAverageRatingAsync()
        {
            if (!await _context.Feedbacks.AnyAsync())
                return 0;

            return await _context.Feedbacks.AverageAsync(f => f.Rating);
}

        public async Task UpdateFeedbackAsync(Feedback updatedFeedback)
        {
            UpdateAsync(updatedFeedback);
            await SaveChangesAsync();
        }

        public async Task<List<Feedback>> GetFeedbacksByInterviewRoomIdAsync(Guid interviewRoomId)
        {
            return await _context.Feedbacks
                .Where(f => f.InterviewRoomId == interviewRoomId)
                .ToListAsync();
        }
    }
}

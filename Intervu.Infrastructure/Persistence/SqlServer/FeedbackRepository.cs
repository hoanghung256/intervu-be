using Azure.Core;
using Intervu.Application.Common;
using Intervu.Application.DTOs.Feedback;
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
    public class FeedbackRepository : RepositoryBase<Feedback>, IFeedbackRepository
    {
        public FeedbackRepository(IntervuDbContext context) : base(context)
        {
        }

        public async Task<PagedResult<Feedback>> GetPagedFeedbacksAsync(int page, int pageSize)
        {
            var query = await _context.Feedbacks.AsQueryable()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Feedback>(query, query.Count, pageSize, page);
        }
        public async Task CreateFeedbackAsync(Feedback feedback)
        {
            await AddAsync(feedback);
            await SaveChangesAsync();
        }

        public async Task<Feedback?> GetFeedbackByIdAsync(int id)
        {
            return await GetByIdAsync(id);
        }


        public async Task<PagedResult<Feedback>> GetFeedbacksByStudentIdAsync(GetFeedbackRequest request)
        {
            var query = _context.Feedbacks.Where(f => f.StudentId == request.StudentId).AsQueryable();
            var totalItems = await query.CountAsync();
            var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
                .ToListAsync();
            return new PagedResult<Feedback>(items, totalItems, request.PageSize, request.Page);
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

        public async Task<List<Feedback>> GetFeedbacksByInterviewRoomIdAsync(int interviewRoomId)
        {
            return await _context.Feedbacks
                .Where(f => f.InterviewRoomId == interviewRoomId)
                .ToListAsync();
        }
    }
}

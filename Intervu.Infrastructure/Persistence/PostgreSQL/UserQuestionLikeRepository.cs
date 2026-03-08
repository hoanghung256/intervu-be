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
    public class UserQuestionLikeRepository : IUserQuestionLikeRepository
    {
        private readonly IntervuPostgreDbContext _context;

        public UserQuestionLikeRepository(IntervuPostgreDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ToggleAsync(Guid userId, Guid questionId)
        {
            var existing = await _context.UserQuestionLikes
                .FirstOrDefaultAsync(l => l.UserId == userId && l.QuestionId == questionId);

            if (existing != null)
            {
                _context.UserQuestionLikes.Remove(existing);
                await _context.SaveChangesAsync();
                return false; // unliked
            }

            await _context.UserQuestionLikes.AddAsync(new UserQuestionLike
            {
                UserId = userId,
                QuestionId = questionId,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            return true; // liked
        }

        public async Task<HashSet<Guid>> GetLikedQuestionIdsAsync(Guid userId, IEnumerable<Guid> questionIds)
        {
            var ids = questionIds.ToList();
            if (!ids.Any()) return new HashSet<Guid>();

            var liked = await _context.UserQuestionLikes
                .Where(l => l.UserId == userId && ids.Contains(l.QuestionId))
                .Select(l => l.QuestionId)
                .ToListAsync();

            return new HashSet<Guid>(liked);
        }

        public async Task<bool> HasLikedAsync(Guid userId, Guid questionId)
        {
            return await _context.UserQuestionLikes
                .AnyAsync(l => l.UserId == userId && l.QuestionId == questionId);
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}

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
    public class UserCommentLikeRepository : IUserCommentLikeRepository
    {
        private readonly IntervuPostgreDbContext _context;

        public UserCommentLikeRepository(IntervuPostgreDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ToggleAsync(Guid userId, Guid commentId)
        {
            var existing = await _context.UserCommentLikes
                .FirstOrDefaultAsync(l => l.UserId == userId && l.CommentId == commentId);

            if (existing != null)
            {
                _context.UserCommentLikes.Remove(existing);
                await _context.SaveChangesAsync();
                return false; // unliked
            }

            await _context.UserCommentLikes.AddAsync(new UserCommentLike
            {
                UserId = userId,
                CommentId = commentId,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            return true; // liked
        }

        public async Task<HashSet<Guid>> GetLikedCommentIdsAsync(Guid userId, IEnumerable<Guid> commentIds)
        {
            var ids = commentIds.ToList();
            if (!ids.Any()) return new HashSet<Guid>();

            var liked = await _context.UserCommentLikes
                .Where(l => l.UserId == userId && ids.Contains(l.CommentId))
                .Select(l => l.CommentId)
                .ToListAsync();

            return new HashSet<Guid>(liked);
        }

        public async Task<bool> HasLikedAsync(Guid userId, Guid commentId)
        {
            return await _context.UserCommentLikes
                .AnyAsync(l => l.UserId == userId && l.CommentId == commentId);
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}

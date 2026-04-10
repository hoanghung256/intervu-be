using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class UserRepository : RepositoryBase<User>, IUserRepository
    {
        public UserRepository(IntervuPostgreDbContext context) : base(context)
        {
        }

        public async Task<User?> GetBySlugAsync(string slug)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.SlugProfileUrl.ToLower() == slug.ToLower());
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<User?> GetByEmailAndPasswordAsync(string email, string password)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.Password == password);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<bool> UpdateProfileAsync(Guid userId, string fullName)
        {
            var user = await GetByIdAsync(userId);
            if (user == null) return false;

            user.FullName = fullName;
            UpdateAsync(user);
            await SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdatePasswordAsync(Guid userId, string hashedPassword)
        {
            var user = await GetByIdAsync(userId);
            if (user == null) return false;

            user.Password = hashedPassword;
            UpdateAsync(user);
            await SaveChangesAsync();
            return true;
        }

        public async Task<string?> UpdateProfilePictureAsync(Guid userId, string profilePictureUrl)
        {
            var user = await GetByIdAsync(userId);
            if (user == null) return null;

            user.ProfilePicture = profilePictureUrl;
            UpdateAsync(user);
            await SaveChangesAsync();
            return profilePictureUrl;
        }

        public async Task<bool> ClearProfilePictureAsync(Guid userId)
        {
            var user = await GetByIdAsync(userId);
            if (user == null) return false;

            user.ProfilePicture = null;
            UpdateAsync(user);
            await SaveChangesAsync();
            return true;
        }

        public async Task<(IReadOnlyList<User> Items, int TotalCount)> GetPagedUsersAsync(int page, int pageSize)
        {
            var query = _context.Users.AsQueryable();

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<(IReadOnlyList<User> Items, int TotalCount)> GetPagedUsersByFilterAsync(int page, int pageSize, UserRole? role, string? search)
        {
            var query = _context.Users.AsQueryable();

            if (role.HasValue)
            {
                query = query.Where(u => u.Role == role.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalized = search.Trim().ToLower();
                query = query.Where(u =>
                    (u.FullName != null && u.FullName.ToLower().Contains(normalized)) ||
                    (u.Email != null && u.Email.ToLower().Contains(normalized)));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public Task<int> GetTotalUsersCountAsync()
        {
            return _context.Users.CountAsync();
        }

        public Task<int> GetActiveUsersCountAsync(DateTime since)
        {
            // Simple MAU logic: users who signed in or updated profile since 'since'
            // In a real system, we'd have a LastActiveAt field. 
            // For now, we'll use CreatedAt and UpdatedAt as a proxy or assume users are active if they exist and we had a login log.
            // Since we don't have Audit Logs for every login here easily, we'll use UpdatedAt.
            return _context.Users.CountAsync(u => u.UpdatedAt >= since || u.CreatedAt >= since);
        }

        public async Task<List<(DateTime Date, int Count)>> GetRegistrationTrendAsync(DateTime from, DateTime to, UserRole? role = null)
        {
            var query = _context.Users.Where(u => u.CreatedAt >= from && u.CreatedAt <= to);
            
            if (role.HasValue)
            {
                query = query.Where(u => u.Role == role.Value);
            }

            var results = await query
                .GroupBy(u => u.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return results.Select(r => (r.Date, r.Count)).ToList();
        }

        public Task<int> GetRegistrationsCountAsync(DateTime start, DateTime end, UserRole? role = null)
        {
            var query = _context.Users.Where(u => u.CreatedAt >= start && u.CreatedAt <= end);
            if (role.HasValue)
            {
                query = query.Where(u => u.Role == role.Value);
            }
            return query.CountAsync();
        }
    }
}

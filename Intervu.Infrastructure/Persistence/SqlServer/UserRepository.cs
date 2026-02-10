using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.SqlServer.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Intervu.Infrastructure.Persistence.SqlServer
{
    public class UserRepository : RepositoryBase<User>, IUserRepository
    {
        public UserRepository(IntervuDbContext context) : base(context)
        {
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

        public async Task<User?> GetBySlugAsync(string slug)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.SlugProfileUrl == slug);
        }

        public async Task<(IReadOnlyList<User> Items, int TotalCount)> GetPagedUsersAsync(int page, int pageSize)
        {
            var query = _context.Users.AsQueryable();

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalItems);
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

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalItems);
        }

        public async Task<int> GetTotalUsersCountAsync()
        {
            return await _context.Users.CountAsync();
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
    }
}

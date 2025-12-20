using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Domain.Entities;
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

        public async Task<int> GetTotalUsersCountAsync()
        {
            return await _context.Users.CountAsync();
        }
        
        public async Task<bool> UpdateProfileAsync(int userId, string fullName)
        {
            var user = await GetByIdAsync(userId);
            if (user == null) return false;

            user.FullName = fullName;
            UpdateAsync(user);
            await SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdatePasswordAsync(int userId, string hashedPassword)
        {
            var user = await GetByIdAsync(userId);
            if (user == null) return false;

            user.Password = hashedPassword;
            UpdateAsync(user);
            await SaveChangesAsync();
            return true;
        }

        public async Task<string?> UpdateProfilePictureAsync(int userId, string profilePictureUrl)
        {
            var user = await GetByIdAsync(userId);
            if (user == null) return null;

            user.ProfilePicture = profilePictureUrl;
            UpdateAsync(user);
            await SaveChangesAsync();
            return profilePictureUrl;
        }
    }
}

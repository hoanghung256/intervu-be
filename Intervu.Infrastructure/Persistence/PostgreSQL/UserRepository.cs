using Intervu.Domain.Entities;
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

        public Task<(IReadOnlyList<User> Items, int TotalCount)> GetPagedUsersAsync(int page, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetTotalUsersCountAsync()
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Application.Common;
using Intervu.Application.Interfaces.Repositories;
using Intervu.Domain.Entities;
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
            return await _context.Users.FirstOrDefaultAsync<User>(u => u.Email.ToLower() == email.ToLower() && u.Password == password);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync<User>(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<PagedResult<User>> GetPagedUsersAsync(int page, int pageSize)
        {
            var query = _context.Users.AsQueryable();

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<User>(items, totalItems, pageSize, page);
        }

        public async Task<int> GetTotalUsersCountAsync()
        {
            return await _context.Users.CountAsync();
        }
    }
}

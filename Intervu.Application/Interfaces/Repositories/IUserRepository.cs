using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Application.Common;
using Intervu.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Intervu.Application.Interfaces.Repositories
{
    public interface IUserRepository : IRepositoryBase<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByEmailAndPasswordAsync(string email, string password);
        Task<bool> EmailExistsAsync(string email);
        Task<PagedResult<User>> GetPagedUsersAsync(int page, int pageSize);
        Task<int> GetTotalUsersCountAsync();
        Task<bool> UpdateProfileAsync(int userId, string fullName);
        Task<bool> UpdatePasswordAsync(int userId, string hashedPassword);
        Task<string?> UpdateProfilePictureAsync(int userId, string profilePictureUrl);
    }
}

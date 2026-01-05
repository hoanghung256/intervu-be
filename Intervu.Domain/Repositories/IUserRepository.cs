using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Domain.Entities;

namespace Intervu.Domain.Repositories
{
    public interface IUserRepository : IRepositoryBase<User>
    {
        Task<User?> GetBySlugAsync(string slug);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByEmailAndPasswordAsync(string email, string password);
        Task<bool> EmailExistsAsync(string email);
        Task<(IReadOnlyList<User> Items, int TotalCount)> GetPagedUsersAsync(int page, int pageSize);
        Task<int> GetTotalUsersCountAsync();
        Task<bool> UpdateProfileAsync(Guid userId, string fullName);
        Task<bool> UpdatePasswordAsync(Guid userId, string hashedPassword);
        Task<string?> UpdateProfilePictureAsync(Guid userId, string profilePictureUrl);
    }
}

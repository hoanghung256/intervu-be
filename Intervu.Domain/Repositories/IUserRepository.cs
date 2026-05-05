using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Domain.Repositories
{
    public interface IUserRepository : IRepositoryBase<User>
    {
        Task<User?> GetBySlugAsync(string slug);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByEmailAndPasswordAsync(string email, string password);
        Task<bool> EmailExistsAsync(string email);
        Task<(IReadOnlyList<User> Items, int TotalCount)> GetPagedUsersAsync(int page, int pageSize);
        Task<(IReadOnlyList<User> Items, int TotalCount)> GetPagedUsersByFilterAsync(int page, int pageSize, UserRole? role, string? search);
        Task<int> GetTotalUsersCountAsync();
        Task<int> GetActiveUsersCountAsync(DateTime since);
        Task<List<(DateTime Date, int Count)>> GetRegistrationTrendAsync(DateTime from, DateTime to, UserRole? role = null);
        Task<int> GetRegistrationsCountAsync(DateTime start, DateTime end, UserRole? role = null);
        Task<bool> UpdateProfileAsync(Guid userId, string fullName);
        Task<bool> UpdatePasswordAsync(Guid userId, string hashedPassword);
        Task<string?> UpdateProfilePictureAsync(Guid userId, string profilePictureUrl);
        Task<bool> ClearProfilePictureAsync(Guid userId);
        /// <summary>Returns the count of active (non-suspended, non-deleted) coach accounts in SQL DB.</summary>
        Task<int> GetActiveCoachCountAsync();

        Task<int?> GetSessionVersionAsync(Guid userId);

        /// <summary>Atomically increments SessionVersion and returns the new value (one round-trip).</summary>
        Task<int> IncrementSessionVersionAndGetAsync(Guid userId);
    }
}


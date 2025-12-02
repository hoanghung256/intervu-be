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
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByEmailAndPasswordAsync(string email, string password);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> UpdateProfileAsync(int userId, string fullName);
        Task<bool> UpdatePasswordAsync(int userId, string hashedPassword);
        Task<string?> UpdateProfilePictureAsync(int userId, string profilePictureUrl);
    }
}

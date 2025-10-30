using AutoMapper;
using Intervu.Application.DTOs.User;
using Intervu.Application.Interfaces.Repositories;
using Intervu.Application.Interfaces.UseCases.Authentication;
using Intervu.Application.Services;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.UseCases.Authentication
{
    public class RegisterUseCase : IRegisterUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public RegisterUseCase(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<bool> ExecuteAsync(RegisterRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return false;
            }

            var emailExists = await _userRepository.EmailExistsAsync(request.Email);
            if (emailExists)
            {
                return false;
            }

            var user = _mapper.Map<User>(request);
            user.Password = PasswordHashHandler.HashPassword(request.Password);

            // Parse role from string to enum
            if (Enum.TryParse<UserRole>(request.Role, out var role))
            {
                user.Role = role;
            }
            else
            {
                user.Role = UserRole.Interviewee; // Default role
            }

            // Set default active status
            user.Status = UserStatus.Active;

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return true;
        }
    }
}

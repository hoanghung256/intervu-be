using Intervu.Application.DTOs.User;

namespace Intervu.Application.Interfaces.UseCases.Authentication
{
    public interface IGoogleLoginUseCase
    {
        Task<LoginResponse> ExecuteAsync(string idToken);
    }
}
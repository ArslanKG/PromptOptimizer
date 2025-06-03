using PromptOptimizer.Core.DTOs;

namespace PromptOptimizer.Core.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);

        Task LogoutAsync(string token);

        Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    }
}
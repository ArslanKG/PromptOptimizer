using System.Security.Claims;
using PromptOptimizer.Core.DTOs;

namespace PromptOptimizer.Core.Interfaces
{
    public interface IJwtTokenService
    {
        TokenResponse GenerateTokens(User user);

        ClaimsPrincipal? ValidateToken(string token);

        string GetUserIdFromToken(string token);
    }
}
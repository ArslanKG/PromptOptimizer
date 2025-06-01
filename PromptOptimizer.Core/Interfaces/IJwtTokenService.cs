using PromptOptimizer.Core.DTOs;
using PromptOptimizer.Core.Entities;
using System.Security.Claims;

namespace PromptOptimizer.Core.Interfaces
{
    public interface IJwtTokenService
    {
        TokenResponse GenerateTokens(User user);
        ClaimsPrincipal? ValidateToken(string token);
        string GetUserIdFromToken(string token);
    }
}
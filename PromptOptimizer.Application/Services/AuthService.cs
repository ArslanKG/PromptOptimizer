using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PromptOptimizer.Core.DTOs;
using PromptOptimizer.Core.Entities;
using PromptOptimizer.Core.Interfaces;
using PromptOptimizer.Infrastructure.Data;
using System.Linq;

namespace PromptOptimizer.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHashingService _passwordHashingService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILogger<AuthService> _logger;
        private readonly Dictionary<string, RefreshTokenInfo> _refreshTokens = new();

        public AuthService(
            AppDbContext context,
            IPasswordHashingService passwordHashingService,
            IJwtTokenService jwtTokenService,
            ILogger<AuthService> logger)
        {
            _context = context;
            _passwordHashingService = passwordHashingService;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

                if (user == null || !_passwordHashingService.VerifyPassword(request.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Failed login attempt for username: {Username}", request.Username);
                    throw new UnauthorizedAccessException("Invalid username or password");
                }

                var tokenResponse = _jwtTokenService.GenerateTokens(user);

                // Store refresh token
                _refreshTokens[tokenResponse.RefreshToken] = new RefreshTokenInfo
                {
                    UserId = user.Id,
                    ExpiryDate = DateTime.UtcNow.AddDays(7)
                };

                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("User {Username} logged in successfully", user.Username);

                return new LoginResponse
                {
                    Success = true,
                    Token = tokenResponse.AccessToken,
                    RefreshToken = tokenResponse.RefreshToken,
                    ExpiresIn = tokenResponse.ExpiresIn,
                    Username = user.Username,
                    Email = user.Email,
                    IsAdmin = user.IsAdmin
                };
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for username: {Username}", request.Username);
                throw new InvalidOperationException("An error occurred during login");
            }
        }

        public async Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            if (!_refreshTokens.TryGetValue(request.RefreshToken, out var tokenInfo))
            {
                throw new UnauthorizedAccessException("Invalid refresh token");
            }

            if (tokenInfo.ExpiryDate <= DateTime.UtcNow)
            {
                _refreshTokens.Remove(request.RefreshToken);
                throw new UnauthorizedAccessException("Refresh token expired");
            }

            var user = await _context.Users.FindAsync(tokenInfo.UserId);
            if (user == null || !user.IsActive)
            {
                throw new UnauthorizedAccessException("User not found or inactive");
            }

            // Remove old refresh token
            _refreshTokens.Remove(request.RefreshToken);

            // Generate new tokens
            var newTokens = _jwtTokenService.GenerateTokens(user);

            // Store new refresh token
            _refreshTokens[newTokens.RefreshToken] = new RefreshTokenInfo
            {
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(7)
            };

            return newTokens;
        }

        public Task LogoutAsync(string token)
        {
            // In a production app, you might want to blacklist the token
            // For now, we'll just remove the refresh token if it exists
            var refreshToken = _refreshTokens.FirstOrDefault(x => x.Value.UserId.ToString() == _jwtTokenService.GetUserIdFromToken(token)).Key;
            if (!string.IsNullOrEmpty(refreshToken))
            {
                _refreshTokens.Remove(refreshToken);
            }

            _logger.LogInformation("User logged out");
            return Task.CompletedTask;
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                // Check if username already exists
                if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                {
                    throw new InvalidOperationException("Username already exists");
                }

                // Check if email already exists
                if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                {
                    throw new InvalidOperationException("Email already exists");
                }

                // Create new user
                var user = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    PasswordHash = _passwordHashingService.HashPassword(request.Password),
                    IsActive = true,
                    IsAdmin = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("New user registered: {Username}", user.Username);

                return new RegisterResponse
                {
                    Success = true,
                    Message = "User registered successfully",
                    UserId = user.Id
                };
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                throw new InvalidOperationException("An error occurred during registration");
            }
        }

        private class RefreshTokenInfo
        {
            public int UserId { get; set; }
            public DateTime ExpiryDate { get; set; }
        }
    }
}
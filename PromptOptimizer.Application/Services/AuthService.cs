using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PromptOptimizer.Core.DTOs;
using PromptOptimizer.Core.Entities;
using PromptOptimizer.Core.Interfaces;
using PromptOptimizer.Infrastructure.Data;

namespace PromptOptimizer.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHashingService _passwordHashingService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILogger<AuthService> _logger;

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

                // Store refresh token in database
                var refreshToken = new RefreshToken
                {
                    Token = tokenResponse.RefreshToken,
                    UserId = user.Id,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow
                };

                _context.RefreshTokens.Add(refreshToken);
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

        public async Task LogoutAsync(string token)
        {
            try
            {
                var userId = _jwtTokenService.GetUserIdFromToken(token);
                if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out var userIdInt))
                {
                    // Revoke all active refresh tokens for this user
                    var refreshTokens = await _context.RefreshTokens
                        .Where(rt => rt.UserId == userIdInt && rt.IsActive)
                        .ToListAsync();

                    foreach (var refreshToken in refreshTokens)
                    {
                        refreshToken.IsRevoked = true;
                        refreshToken.RevokedAt = DateTime.UtcNow;
                    }

                    await _context.SaveChangesAsync();
                    _logger.LogInformation("User {UserId} logged out - {TokenCount} refresh tokens revoked",
                        userIdInt, refreshTokens.Count);
                }
                else
                {
                    _logger.LogWarning("Invalid token provided for logout");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
            }
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

    }
}
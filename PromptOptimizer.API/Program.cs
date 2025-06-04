using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using PromptOptimizer.Application.Services;
using PromptOptimizer.Core.Configuration;
using PromptOptimizer.Core.Interfaces;
using PromptOptimizer.Infrastructure.Clients;
using PromptOptimizer.Infrastructure.Data;
using PromptOptimizer.Infrastructure.Services;
using PromptOptimizer.API.Middleware;
using Serilog;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Configure port for Render.com
if (builder.Environment.IsProduction())
{
    builder.WebHost.UseUrls("http://*:10000");
}

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Strategy Configuration
builder.Services.Configure<StrategyConfiguration>(
    builder.Configuration.GetSection("StrategyConfiguration"));

// If not configured in appsettings, use default
if (!builder.Configuration.GetSection("StrategyConfiguration").Exists())
{
    builder.Services.Configure<StrategyConfiguration>(options =>
    {
        var defaultConfig = StrategyConfiguration.GetDefault();
        options.Strategies = defaultConfig.Strategies;
        options.OptimizationTypes = defaultConfig.OptimizationTypes;
        options.ModelStrategies = defaultConfig.ModelStrategies;
    });
}

// Configure Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AI Chat API",
        Version = "v2.0",
        Description = "Simple AI Chat API with multi-model support and session management"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ??
    throw new InvalidOperationException("JWT SecretKey is not configured");

// Validate JWT secret key length for security
if (jwtSecretKey.Length < 32)
{
    throw new InvalidOperationException("JWT Secret key must be at least 32 characters for security");
}

var key = Encoding.UTF8.GetBytes(jwtSecretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "PromptOptimizer",
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "PromptOptimizerUsers",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient<ICortexApiClient, CortexApiClient>(client =>
{
    var timeoutSeconds = builder.Configuration.GetValue<int>("HttpClient:TimeoutSeconds", 30);
    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
    client.DefaultRequestHeaders.Add("Connection", "keep-alive");
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    MaxConnectionsPerServer = 10,
})
.AddPolicyHandler(GetRetryPolicy());

builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<ISessionService, DatabaseSessionService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IValidationService, ValidationService>();
builder.Services.AddScoped<IRateLimitService, RateLimitService>();
builder.Services.AddScoped<ICortexApiClient, CortexApiClient>();
builder.Services.AddScoped<ISessionTitleGenerator, SessionTitleGenerator>();
builder.Services.AddScoped<IPasswordHashingService, PasswordHashingService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

builder.Services.AddMemoryCache();

builder.Services.AddCors(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.AddPolicy("Development", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    }
    else
    {
        options.AddPolicy("Production", policy =>
        {
            var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                               ?? new[] { "https://yourdomain.com" };
            
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    }
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck("database", () =>
    {
        try
        {
            using var scope = builder.Services.BuildServiceProvider().CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.CanConnect();
            return HealthCheckResult.Healthy("Database connection successful");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database connection failed", ex);
        }
    })
    .AddCheck("self", () => HealthCheckResult.Healthy("API is running"));

var app = builder.Build();

// Ensure data directory exists for production
if (app.Environment.IsProduction())
{
    var dataPath = "/app/data";
    if (!Directory.Exists(dataPath))
    {
        Directory.CreateDirectory(dataPath);
    }
    
    var logsPath = "/app/data/logs";
    if (!Directory.Exists(logsPath))
    {
        Directory.CreateDirectory(logsPath);
    }
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Initializing database...");
        context.Database.Migrate();
        logger.LogInformation("Database initialized successfully");

        if (!context.Users.Any(u => u.Username == "admin"))
        {
            logger.LogInformation("Creating admin user...");
            var passwordHashingService = scope.ServiceProvider.GetRequiredService<IPasswordHashingService>();

            var adminPassword = builder.Configuration["AdminSetup:Password"] ?? "ChangeThisPassword123!";
            var adminEmail = builder.Configuration["AdminSetup:Email"] ?? "admin@example.com";

            context.Users.Add(new User
            {
                Username = "admin",
                Email = adminEmail,
                PasswordHash = passwordHashingService.HashPassword(adminPassword),
                IsAdmin = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            });

            await context.SaveChangesAsync();
            logger.LogInformation("Admin user created successfully");
            logger.LogWarning("Default admin credentials are being used. Please change them in production!");
        }
        else
        {
            logger.LogInformation("Admin user already exists");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while initializing the database");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Enable Swagger in production for Render.com
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PromptOptimizer API v2.0");
        c.RoutePrefix = "swagger";
    });
}

// Global exception handling middleware (should be early in pipeline)
app.UseMiddleware<GlobalExceptionMiddleware>();

// Only use HTTPS redirection in development (Render.com handles HTTPS)
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Use environment-specific CORS policy
if (app.Environment.IsDevelopment())
{
    app.UseCors("Development");
}
else
{
    app.UseCors("Production");
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Add health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/api/system/health");

app.Lifetime.ApplicationStarted.Register(() =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    var urls = app.Urls;
    var env = app.Environment.EnvironmentName;

    logger.LogInformation("========================================");
    logger.LogInformation("AI Chat API started successfully!");
    logger.LogInformation($"Environment: {env}");
    logger.LogInformation("Listening on:");
    foreach (var url in urls)
    {
        logger.LogInformation($"   - {url}");
        if (url.Contains("https"))
        {
            logger.LogInformation($"   - {url}/swagger (Swagger UI)");
        }
    }
    logger.LogInformation("========================================");
    logger.LogInformation("Available endpoints:");
    logger.LogInformation("   - POST   /api/auth/login");
    logger.LogInformation("   - POST   /api/auth/register");
    logger.LogInformation("   - POST   /api/chat/send");      
    logger.LogInformation("   - GET    /api/chat/models");        
    logger.LogInformation("   - GET    /api/chat/sessions");       
    logger.LogInformation("   - GET    /api/chat/health");         
    logger.LogInformation("========================================");
    logger.LogInformation("Press Ctrl+C to shut down");
});

try
{
    Log.Information("Starting AI Chat API...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => !msg.IsSuccessStatusCode)
        .WaitAndRetryAsync(
            3,
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                var logger = Log.ForContext<Program>();
                logger.Warning("Delaying for {Delay}ms, then making retry {Retry}.",
                    timespan.TotalMilliseconds, retryCount);
            });
}
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using PromptOptimizer.Application.Services;
using PromptOptimizer.Core.Interfaces;
using PromptOptimizer.Infrastructure.Clients;
using PromptOptimizer.Infrastructure.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog'u doðru þekilde yapýlandýr - sadece bir kez
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext());

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PromptOptimizer API",
        Version = "v1",
        Description = "Multi-model AI prompt optimization API with authentication"
    });

    // Add JWT Authentication
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

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure JWT Authentication
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");
var key = Encoding.ASCII.GetBytes(jwtSecretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in production
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

// Add HttpClient with Polly
builder.Services.AddHttpClient<ICortexApiClient, CortexApiClient>()
    .AddPolicyHandler(GetRetryPolicy());

// Register services
builder.Services.AddScoped<ISessionService, DatabaseSessionService>();
builder.Services.AddScoped<IPromptOptimizerService, PromptOptimizerService>();
builder.Services.AddScoped<IModelOrchestrator, ModelOrchestrator>();
builder.Services.AddScoped<IOptimizationService, OptimizationService>();
builder.Services.AddScoped<IPasswordHashingService, PasswordHashingService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISessionManagementService, SessionManagementService>();
builder.Services.AddScoped<IValidationService, ValidationService>();
builder.Services.AddScoped<IRateLimitService, RateLimitService>();

// Add memory cache
builder.Services.AddMemoryCache();

builder.Services.AddScoped<ISessionCacheService, SessionCacheService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

// Ensure database is created and seeded
// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Initializing database...");

        // Migration'larý uygula
        context.Database.Migrate();

        logger.LogInformation("Database initialized successfully");

        if (!context.Users.Any(u => u.Username == "admin"))
        {
            logger.LogInformation("Creating admin user...");
            var passwordHashingService = scope.ServiceProvider.GetRequiredService<IPasswordHashingService>();

            // Þifre configuration'dan alýnsýn
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Add startup complete message
app.Lifetime.ApplicationStarted.Register(() =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    var urls = app.Urls;
    var env = app.Environment.EnvironmentName;

    logger.LogInformation("========================================");
    logger.LogInformation("PromptOptimizer API started successfully!");
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
    logger.LogInformation("   - POST   /api/optimization/optimize");
    logger.LogInformation("   - GET    /api/optimization/models");
    logger.LogInformation("   - GET    /api/optimization/strategies");
    logger.LogInformation("========================================");
    logger.LogInformation("Press Ctrl+C to shut down");
});

try
{
    Log.Information("Starting PromptOptimizer API...");
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
                logger.Warning("Delaying for {Delay}ms, then making retry {Retry}.", timespan.TotalMilliseconds, retryCount);
            });
}
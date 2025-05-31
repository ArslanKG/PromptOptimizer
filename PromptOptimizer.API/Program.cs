using PromptOptimizer.Application.Services;
using PromptOptimizer.Core.Interfaces;
using PromptOptimizer.Infrastructure.Clients;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Prompt Optimizer API",
        Version = "v1",
        Description = "Multi-model AI prompt optimization service"
    });

    // XML documentation satýrýný kaldýrdýk çünkü dosya yok
});

// Configure HttpClient for CortexAPI
builder.Services.AddHttpClient<ICortexApiClient, CortexApiClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Register application services
builder.Services.AddScoped<IPromptOptimizerService, PromptOptimizerService>();
builder.Services.AddScoped<IModelOrchestrator, ModelOrchestrator>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add response caching
builder.Services.AddResponseCaching();

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Prompt Optimizer API V1");
        c.RoutePrefix = string.Empty; // Swagger'ý root'ta aç
    });
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseResponseCaching();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// Log application start
Log.Information("Prompt Optimizer API started successfully");

app.Run();
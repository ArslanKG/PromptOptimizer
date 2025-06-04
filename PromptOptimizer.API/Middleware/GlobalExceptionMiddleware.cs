using System.Net;
using System.Text.Json;
using PromptOptimizer.Core.DTOs;

namespace PromptOptimizer.API.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public GlobalExceptionMiddleware(
            RequestDelegate next, 
            ILogger<GlobalExceptionMiddleware> logger,
            IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred. TraceId: {TraceId}", context.TraceIdentifier);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var (statusCode, errorCode, message, details) = GetErrorResponse(exception);
            
            context.Response.StatusCode = (int)statusCode;

            var response = new ErrorResponse(
                errorCode, 
                message, 
                _environment.IsDevelopment() ? details : null
            );

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = _environment.IsDevelopment()
            };

            var jsonResponse = JsonSerializer.Serialize(response, jsonOptions);
            await context.Response.WriteAsync(jsonResponse);
        }

        private static (HttpStatusCode statusCode, string errorCode, string message, string? details) GetErrorResponse(Exception exception)
        {
            return exception switch
            {
                ArgumentNullException => (
                    HttpStatusCode.BadRequest,
                    "REQUIRED_PARAMETER_MISSING",
                    "Required parameter is missing",
                    exception.ToString()
                ),
                ArgumentException => (
                    HttpStatusCode.BadRequest,
                    "INVALID_ARGUMENT",
                    exception.Message,
                    exception.ToString()
                ),
                UnauthorizedAccessException => (
                    HttpStatusCode.Unauthorized,
                    "ACCESS_DENIED",
                    exception.Message,
                    exception.ToString()
                ),
                InvalidOperationException => (
                    HttpStatusCode.BadRequest,
                    "INVALID_OPERATION",
                    exception.Message,
                    exception.ToString()
                ),
                KeyNotFoundException => (
                    HttpStatusCode.NotFound,
                    "RESOURCE_NOT_FOUND",
                    "The requested resource was not found",
                    exception.ToString()
                ),
                TimeoutException => (
                    HttpStatusCode.RequestTimeout,
                    "REQUEST_TIMEOUT",
                    "The request timed out",
                    exception.ToString()
                ),
                HttpRequestException => (
                    HttpStatusCode.BadGateway,
                    "EXTERNAL_SERVICE_ERROR",
                    "External service error",
                    exception.ToString()
                ),
                TaskCanceledException => (
                    HttpStatusCode.RequestTimeout,
                    "REQUEST_CANCELLED",
                    "The request was cancelled",
                    exception.ToString()
                ),
                _ => (
                    HttpStatusCode.InternalServerError,
                    "INTERNAL_SERVER_ERROR",
                    "An internal server error occurred",
                    exception.ToString()
                )
            };
        }
    }
}
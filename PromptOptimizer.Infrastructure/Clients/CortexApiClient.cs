using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PromptOptimizer.Core.DTOs;
using PromptOptimizer.Core.Interfaces;

namespace PromptOptimizer.Infrastructure.Clients;

public class CortexApiClient : ICortexApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CortexApiClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public CortexApiClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<CortexApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        var apiKey = configuration["CortexApi:ApiKey"]
            ?? throw new InvalidOperationException("CortexApi:ApiKey not configured");

        _httpClient.BaseAddress = new Uri("https://api.claude.gg/v1/");
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    public async Task<ChatCompletionResponse> CreateChatCompletionAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Sending request to CortexAPI with model: {Model}", request.Model);

            var response = await _httpClient.PostAsync(
                "chat/completions",
                content,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<ChatCompletionResponse>(responseJson, _jsonOptions)
                ?? throw new InvalidOperationException("Failed to deserialize response");

            _logger.LogInformation(
                "Received response from CortexAPI. Tokens used: {TotalTokens}",
                result.Usage?.TotalTokens ?? 0);

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed");
            throw;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request timeout");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in CortexAPI client");
            throw;
        }
    }
}
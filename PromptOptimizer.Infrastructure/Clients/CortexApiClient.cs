using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PromptOptimizer.Core.DTOs;
using PromptOptimizer.Core.Interfaces;

namespace PromptOptimizer.Infrastructure.Clients
{
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
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        public async Task<ChatCompletionResponse> CreateChatCompletionAsync(
            ChatCompletionRequest request,
            CancellationToken cancellationToken = default)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                if (request.Messages.Count == 0)
                {
                    throw new ArgumentException("Messages array cannot be empty");
                }

                _logger.LogInformation("Sending request to CortexAPI - Model: {Model}, Messages: {Count}",
                    request.Model, request.Messages.Count);

                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(30));

                var response = await _httpClient.PostAsync("chat/completions", content, cts.Token);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("CortexAPI error {StatusCode}: {Error}", response.StatusCode, errorContent);
                    throw new HttpRequestException($"API Error {response.StatusCode}: {errorContent}");
                }

                var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                var chatResponse = JsonSerializer.Deserialize<ChatCompletionResponse>(responseJson, _jsonOptions);

                stopwatch.Stop();
                _logger.LogInformation("CortexAPI responded in {Ms}ms - Tokens: {Tokens}",
                    stopwatch.ElapsedMilliseconds, chatResponse?.Usage?.TotalTokens ?? 0);

                return chatResponse ?? new ChatCompletionResponse();
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "CortexAPI request failed after {Ms}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }
}
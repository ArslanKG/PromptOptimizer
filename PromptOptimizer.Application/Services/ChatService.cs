// PromptOptimizer.Application/Services/ChatService.cs
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PromptOptimizer.Core.Configuration;
using PromptOptimizer.Core.DTOs;
using PromptOptimizer.Core.Entities;
using PromptOptimizer.Core.Interfaces;

namespace PromptOptimizer.Application.Services
{
    public class ChatService : IChatService
    {
        private readonly ICortexApiClient _cortexClient;
        private readonly ISessionService _sessionService;
        private readonly ISessionTitleGenerator _titleGenerator;
        private readonly ILogger<ChatService> _logger;
        private readonly StrategyConfiguration _strategyConfig;


        // PromptOptimizer.Application/Services/ChatService.cs - Update SupportedModels
        private static readonly List<AvailableModel> SupportedModels = new()
        {
            // OpenAI Models
            new() { Name = "gpt-4o-mini", DisplayName = "GPT-4o Mini (Fast & Affordable)", CostPer1KTokens = 0.15m, IsRecommended = true },
            new() { Name = "gpt-4o", DisplayName = "GPT-4o (Premium Quality)", CostPer1KTokens = 1.00m, IsRecommended = false },
            new() { Name = "o3-mini", DisplayName = "O3 Mini (Advanced Reasoning)", CostPer1KTokens = 0.20m, IsRecommended = false },
    
            // Google Models
            new() { Name = "gemini", DisplayName = "Gemini (Google AI)", CostPer1KTokens = 0.50m, IsRecommended = false },
            new() { Name = "gemini-lite", DisplayName = "Gemini Lite (Fast & Light)", CostPer1KTokens = 0.25m, IsRecommended = false },
    
            // DeepSeek Models
            new() { Name = "deepseek-chat", DisplayName = "DeepSeek Chat (Coding)", CostPer1KTokens = 0.30m, IsRecommended = false },
            new() { Name = "deepseek-r1", DisplayName = "DeepSeek R1 (Reasoning)", CostPer1KTokens = 0.80m, IsRecommended = false },
    
            // xAI Models
            new() { Name = "grok-3-mini-beta", DisplayName = "Grok 3 Mini Beta (xAI)", CostPer1KTokens = 0.25m, IsRecommended = false },
            new() { Name = "grok-2", DisplayName = "Grok 2 (xAI Premium)", CostPer1KTokens = 0.90m, IsRecommended = false }
        };

        public ChatService(
            ICortexApiClient cortexClient,
            ISessionService sessionService,
            ISessionTitleGenerator titleGenerator,
            IOptions<StrategyConfiguration> strategyOptions,
            ILogger<ChatService> logger)
        {
            _cortexClient = cortexClient;
            _sessionService = sessionService;
            _titleGenerator = titleGenerator;
            _strategyConfig = strategyOptions.Value;
            _logger = logger;
        }

        public async Task<ChatResponse> SendMessageAsync(
            string message,
            string model,
            string? sessionId = null,
            double temperature = 0.7,
            int? maxTokens = null,
            int? userId = null,
            CancellationToken cancellationToken = default)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Processing chat - Model: {Model}, HasSession: {HasSession}",
                    model, !string.IsNullOrEmpty(sessionId));

                var session = await EnsureSessionAsync(sessionId, userId);
                var isNewSession = string.IsNullOrEmpty(sessionId);

                var contextTask = BuildCompleteContext(session.SessionId, message);
                var contextMessages = await contextTask;

                _logger.LogDebug("Context built in {Ms}ms", stopwatch.ElapsedMilliseconds);

                var aiRequest = new ChatCompletionRequest
                {
                    Model = model,
                    Messages = contextMessages,
                    Temperature = temperature,
                    MaxTokens = maxTokens
                };

                var aiCallStart = stopwatch.ElapsedMilliseconds;
                var aiResponse = await _cortexClient.CreateChatCompletionAsync(aiRequest, cancellationToken);
                var aiCallTime = stopwatch.ElapsedMilliseconds - aiCallStart;

                _logger.LogInformation("AI API call took {Ms}ms", aiCallTime);

                var assistantMessage = ExtractAssistantMessage(aiResponse);

                var saveTask = SaveConversationMessagesAsync(session.SessionId, message, assistantMessage);
                var titleTask = isNewSession
                    ? GenerateAndUpdateSessionTitle(session.SessionId, message)
                    : Task.FromResult<string?>(null);

                await Task.WhenAll(saveTask, titleTask);
                var sessionTitle = await titleTask;

                stopwatch.Stop();
                _logger.LogInformation("Total request time: {Ms}ms (AI: {AiMs}ms)",
                    stopwatch.ElapsedMilliseconds, aiCallTime);

                return new ChatResponse
                {
                    SessionId = session.SessionId,
                    Message = assistantMessage,
                    Model = model,
                    Usage = aiResponse.Usage,
                    Success = true,
                    SessionTitle = sessionTitle,
                    IsNewSession = isNewSession
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Chat failed after {Ms}ms - Model: {Model}",
                    stopwatch.ElapsedMilliseconds, model);
                throw;
            }
        }

        private async Task SaveConversationMessagesAsync(string sessionId, string userMessage, string assistantMessage)
        {
            try
            {
                var userTask = _sessionService.AddMessageAsync(sessionId, "user", userMessage);
                var assistantTask = _sessionService.AddMessageAsync(sessionId, "assistant", assistantMessage);

                await Task.WhenAll(userTask, assistantTask);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save messages for session {SessionId}", sessionId);
                throw;
            }
        }

        public async Task<ChatResponse> SendMessageWithStrategyAsync(
            string message,
            string strategy = "default",
            string? sessionId = null,
            int? userId = null,
            CancellationToken cancellationToken = default)
        {
            var modelConfig = GetModelConfigForStrategy(strategy);

            _logger.LogInformation("Using strategy: {Strategy} → Model: {Model}", strategy, modelConfig.Name);

            return await SendMessageAsync(
                message,
                modelConfig.Name,
                sessionId,
                modelConfig.Temperature,
                modelConfig.MaxTokens,
                userId,
                cancellationToken);
        }

        public Task<List<AvailableModel>> GetAvailableModelsAsync()
        {
            return Task.FromResult(SupportedModels);
        }

        public Task<bool> ValidateModelAsync(string modelName)
        {
            var supportedModelNames = new[]
            {
                "gpt-4o-mini", "gpt-4o", "o3-mini",
                "gemini", "gemini-lite",
                "deepseek-chat", "deepseek-r1",
                "grok-3-mini-beta", "grok-2"
            };

            var isSupported = supportedModelNames.Contains(modelName, StringComparer.OrdinalIgnoreCase);
            return Task.FromResult(isSupported);
        }

        // 🔥 COMPLETELY NEW: Proper context building with conversation history
        private async Task<List<ChatMessage>> BuildCompleteContext(string sessionId, string currentMessage)
        {
            try
            {
                _logger.LogDebug("Building complete context for session {SessionId}", sessionId);

                // Get conversation history from database
                var conversationHistory = await _sessionService.GetConversationContextAsync(sessionId, 8);

                _logger.LogDebug("Retrieved {Count} historical messages from database", conversationHistory.Count);

                // Convert to ChatMessage format for AI API
                var contextMessages = new List<ChatMessage>();

                foreach (var historyMsg in conversationHistory.OrderBy(m => m.Timestamp))
                {
                    contextMessages.Add(new ChatMessage
                    {
                        Role = historyMsg.Role,
                        Content = historyMsg.Content
                    });

                    _logger.LogDebug("Added to context: {Role} - '{Preview}'",
                        historyMsg.Role, historyMsg.Content[..Math.Min(50, historyMsg.Content.Length)]);
                }

                // Add current user message
                contextMessages.Add(new ChatMessage
                {
                    Role = "user",
                    Content = currentMessage
                });

                _logger.LogInformation("Final context: {Count} messages (including {HistoryCount} from history)",
                    contextMessages.Count, conversationHistory.Count);

                return contextMessages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to build context for session {SessionId}", sessionId);

                // Fallback: just current message
                return new List<ChatMessage>
                {
                    new() { Role = "user", Content = currentMessage }
                };
            }
        }

        private async Task<ConversationSession> EnsureSessionAsync(string? sessionId, int? userId)
        {
            if (!string.IsNullOrEmpty(sessionId))
            {
                var existingSession = await _sessionService.GetSessionAsync(sessionId);
                if (existingSession != null)
                {
                    _logger.LogDebug("Using existing session: {SessionId}", sessionId);
                    return existingSession;
                }
                _logger.LogWarning("Session {SessionId} not found, creating new session", sessionId);
            }

            var newSession = await _sessionService.CreateSessionAsync(userId);
            _logger.LogInformation("Created new session: {SessionId}", newSession.SessionId);
            return newSession;
        }

        private string ExtractAssistantMessage(ChatCompletionResponse response)
        {
            var assistantMessage = response.Choices?.FirstOrDefault()?.Message?.Content ?? "";

            if (string.IsNullOrEmpty(assistantMessage))
            {
                throw new InvalidOperationException("Empty AI response received");
            }

            return assistantMessage;
        }


        private async Task<string?> GenerateAndUpdateSessionTitle(string sessionId, string firstMessage)
        {
            try
            {
                var sessionTitle = _titleGenerator.GenerateTitle(firstMessage);
                await UpdateSessionTitle(sessionId, sessionTitle);

                _logger.LogInformation("Generated session title: '{Title}' for session {SessionId}",
                    sessionTitle, sessionId);

                return sessionTitle;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to generate/update session title for {SessionId}", sessionId);
                return null;
            }
        }

        private async Task UpdateSessionTitle(string sessionId, string title)
        {
            try
            {
                await _sessionService.UpdateSessionTitleAsync(sessionId, title);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update session title for {SessionId}", sessionId);
            }
        }

        private ModelConfig GetModelConfigForStrategy(string strategy)
        {
            return _strategyConfig.ModelStrategies.GetValueOrDefault(strategy.ToLower(),
                _strategyConfig.ModelStrategies["default"]);
        }
    }
}
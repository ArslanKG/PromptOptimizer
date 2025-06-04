using System.Text.Json.Serialization;

namespace PromptOptimizer.Core.DTOs
{
    public class ConversationMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("model")]
        public string? Model { get; set; }
    }
}
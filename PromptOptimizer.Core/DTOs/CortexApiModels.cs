namespace PromptOptimizer.Core.DTOs;

public class ChatCompletionRequest
{
    public string Model { get; set; } = string.Empty;
    public List<ChatMessage> Messages { get; set; } = new();
    public double Temperature { get; set; } = 0.7;
    public int? MaxTokens { get; set; }
}

public class ChatMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class ChatCompletionResponse
{
    public string Id { get; set; } = string.Empty;
    public string Object { get; set; } = string.Empty;
    public long Created { get; set; }
    public string Model { get; set; } = string.Empty;
    public List<Choice> Choices { get; set; } = new();
    public Usage? Usage { get; set; }
}

public class Choice
{
    public ChatMessage Message { get; set; } = new();
    public int Index { get; set; }
    public string? FinishReason { get; set; }
}

public class Usage
{
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int TotalTokens { get; set; }
}
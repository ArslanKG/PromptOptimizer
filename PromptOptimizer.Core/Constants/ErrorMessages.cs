namespace PromptOptimizer.Core.Constants;

public static class ErrorMessages
{
    public const string PromptCannotBeEmpty = "Prompt cannot be empty";
    public const string RequestTimeout = "Request timeout";
    public const string ProcessingError = "An error occurred while processing your request";
    public const string NoResponseFromModel = "No response from {0}";
    public const string NoValidResponses = "No valid responses from consensus models";
    public const string SynthesisFailed = "Synthesis failed - no response content";
}
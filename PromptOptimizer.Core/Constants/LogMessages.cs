namespace PromptOptimizer.Core.Constants
{
    public static class LogMessages
    {
        public const string ProcessingOptimization = "Processing optimization request with strategy: {Strategy}";
        public const string FetchingModels = "Fetching available models";
        public const string ReturningStrategies = "Returning {Count} strategies";
        public const string OptimizedPrompt = "Optimized prompt: {OptimizedPrompt}";
        public const string EmptyResponse = "Initial response is empty from model {Model}";
        public const string ProcessingCompleted = "Processed prompt with strategy {Strategy} in {ElapsedMs}ms using models: {Models}";
        public const string StartingConsensus = "Starting consensus strategy with models: {Models}";
        public const string ConsensusSuccess = "All consensus models responded successfully";
        public const string SynthesizingResponses = "Synthesizing responses with {Model}";
        public const string UsingCheapestModel = "Using cheapest model: {Model} with cost: {Cost}";
        public const string ErrorInStrategy = "Error in {Strategy}";
    }
}
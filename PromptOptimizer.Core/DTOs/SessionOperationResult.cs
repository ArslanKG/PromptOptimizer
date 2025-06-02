namespace PromptOptimizer.Core.DTOs
{
    public class SessionOperationResult<T>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public OperationStatus Status { get; set; }

        public static SessionOperationResult<T> Success(T data) => new()
        {
            IsSuccess = true,
            Data = data,
            Status = OperationStatus.Success
        };

        public static SessionOperationResult<T> Failure(string errorMessage) => new()
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            Status = OperationStatus.BadRequest
        };

        public static SessionOperationResult<T> NotFound(string errorMessage) => new()
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            Status = OperationStatus.NotFound
        };

        public static SessionOperationResult<T> Forbidden(string errorMessage) => new()
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            Status = OperationStatus.Forbidden
        };

        public static SessionOperationResult<T> Error(string errorMessage) => new()
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            Status = OperationStatus.InternalError
        };
    }

    public enum OperationStatus
    {
        Success,
        BadRequest,
        NotFound,
        Forbidden,
        InternalError
    }
}
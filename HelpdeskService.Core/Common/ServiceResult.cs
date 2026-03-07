namespace HelpdeskService.Core.Common;

public enum ServiceResultError
{
    None,
    NotFound,
    Conflict,
    Forbidden,
    Unauthorized
}

public class ServiceResult<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public ServiceResultError Error { get; }
    public string ErrorMessage { get; }

    private ServiceResult(T value)
    {
        IsSuccess = true;
        Value = value;
        Error = ServiceResultError.None;
        ErrorMessage = string.Empty;
    }

    private ServiceResult(ServiceResultError error, string message)
    {
        IsSuccess = false;
        Value = default;
        Error = error;
        ErrorMessage = message;
    }

    public static ServiceResult<T> Success(T value) => new(value);

    public static ServiceResult<T> NotFound(string message = "Resource not found.") =>
        new(ServiceResultError.NotFound, message);

    public static ServiceResult<T> Conflict(string message = "Resource already exists.") =>
        new(ServiceResultError.Conflict, message);

    public static ServiceResult<T> Forbidden(string message = "Access denied.") =>
        new(ServiceResultError.Forbidden, message);

    public static ServiceResult<T> Unauthorized(string message = "Invalid credentials.") =>
        new(ServiceResultError.Unauthorized, message);
}

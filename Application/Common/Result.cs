namespace OtpSystem.Application.Common;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public string? ErrorCode { get; }

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
    }

    private Result(string errorCode, string error)
    {
        IsSuccess = false;
        ErrorCode = errorCode;
        Error = error;
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(string errorCode, string error) => new(errorCode, error);
}

// Non-generic version for operations with no return value
public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public string? ErrorCode { get; }

    private Result(bool success, string? errorCode = null, string? error = null)
    {
        IsSuccess = success;
        ErrorCode = errorCode;
        Error = error;
    }

    public static Result Success() => new(true);
    public static Result Failure(string errorCode, string error) => new(false, errorCode, error);
}
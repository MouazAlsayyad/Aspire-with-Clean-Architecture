namespace AspireApp.Domain.Shared.Common;

/// <summary>
/// Represents an error that occurred during an operation.
/// This is a value object that encapsulates error information.
/// </summary>
public class Error
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);
    public static readonly Error NullValue = new("General.NullValue", "The result value is null.", ErrorType.Validation);

    public string Code { get; }
    public string Message { get; }
    public ErrorType Type { get; }

    protected Error(string code, string message, ErrorType type)
    {
        Code = code;
        Message = message;
        Type = type;
    }

    public static Error Create(string code, string message, ErrorType type = ErrorType.Validation)
    {
        return new Error(code, message, type);
    }

    public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);
    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);
    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);
    public static Error Unauthorized(string code, string message) => new(code, message, ErrorType.Unauthorized);
    public static Error Forbidden(string code, string message) => new(code, message, ErrorType.Forbidden);
    public static Error Failure(string code, string message) => new(code, message, ErrorType.Failure);
}

/// <summary>
/// Represents the type of error that occurred.
/// </summary>
public enum ErrorType
{
    None = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Unauthorized = 4,
    Forbidden = 5,
    Failure = 6
}


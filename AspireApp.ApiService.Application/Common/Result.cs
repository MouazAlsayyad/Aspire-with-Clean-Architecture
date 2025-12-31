using AspireApp.ApiService.Domain.Common;

namespace AspireApp.ApiService.Application.Common;

/// <summary>
/// Represents the result of an operation that can either succeed with a value or fail with an error.
/// This is a functional approach to error handling that avoids exceptions for control flow.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("A successful result cannot have an error.");

        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("A failed result must have an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);

    public static Result<T> Success<T>(T value) => new(value, true, Error.None);
    public static Result<T> Failure<T>(Error error) => new(default!, false, error);

    public static implicit operator Result(Error error) => Failure(error);
}

/// <summary>
/// Represents the result of an operation that can either succeed with a value of type T or fail with an error.
/// </summary>
public class Result<T> : Result
{
    private readonly T _value;

    internal Result(T value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public T Value => IsSuccess
        ? _value
        : throw new InvalidOperationException("Cannot access the value of a failed result.");

    /// <summary>
    /// Pagination metadata (optional, only set for paginated results)
    /// </summary>
    public PaginationInfo? Pagination { get; private set; }

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure<T>(error);

    /// <summary>
    /// Creates a successful result with pagination metadata
    /// </summary>
    public static Result<T> Success(T value, PaginationInfo pagination)
    {
        var result = new Result<T>(value, true, Error.None);
        result.Pagination = pagination;
        return result;
    }

    /// <summary>
    /// Creates a successful result with pagination metadata
    /// </summary>
    public static Result<T> Success(T value, long totalCount, int pageNumber, int pageSize)
    {
        var result = new Result<T>(value, true, Error.None);
        result.Pagination = new PaginationInfo(totalCount, pageNumber, pageSize);
        return result;
    }

    /// <summary>
    /// Executes an action if the result is successful.
    /// </summary>
    public Result<T> OnSuccess(Action<T> action)
    {
        if (IsSuccess)
        {
            action(_value);
        }
        return this;
    }

    /// <summary>
    /// Executes an action if the result is a failure.
    /// </summary>
    public Result<T> OnFailure(Action<Error> action)
    {
        if (IsFailure)
        {
            action(Error);
        }
        return this;
    }

    /// <summary>
    /// Maps the value to another type if the result is successful.
    /// </summary>
    public Result<TOut> Map<TOut>(Func<T, TOut> mapper)
    {
        return IsSuccess
            ? Result.Success(mapper(_value))
            : Result.Failure<TOut>(Error);
    }

    /// <summary>
    /// Binds the value to another result if the result is successful.
    /// </summary>
    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> binder)
    {
        return IsSuccess
            ? binder(_value)
            : Result.Failure<TOut>(Error);
    }

    /// <summary>
    /// Matches the result and executes the appropriate function.
    /// </summary>
    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<Error, TOut> onFailure)
    {
        return IsSuccess
            ? onSuccess(_value)
            : onFailure(Error);
    }
}


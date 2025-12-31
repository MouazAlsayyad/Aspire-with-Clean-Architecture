namespace AspireApp.ApiService.Domain.Common;

/// <summary>
/// Base exception for domain-related errors.
/// Domain services should throw this exception (or derived exceptions) for business rule violations.
/// </summary>
public class DomainException : Exception
{
    public Error Error { get; }

    public DomainException(Error error) : base(error.Message)
    {
        Error = error;
    }

    public DomainException(Error error, Exception innerException) : base(error.Message, innerException)
    {
        Error = error;
    }
}


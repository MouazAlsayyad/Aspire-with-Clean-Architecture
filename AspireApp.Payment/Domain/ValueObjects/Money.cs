using System;
using AspireApp.Domain.Shared.Common;

namespace AspireApp.Payment.Domain.ValueObjects;

/// <summary>
/// Money value object representing a monetary amount with currency
/// </summary>
public sealed record Money(decimal Amount, Currency Currency)
{
    /// <summary>
    /// Adds two Money values with the same currency
    /// </summary>
    /// <exception cref="DomainException">Thrown when currencies differ</exception>
    public static Money operator +(Money first, Money second)
    {
        if (first.Currency != second.Currency)
        {
            throw new DomainException(
                DomainErrors.General.InvalidInput($"Cannot add money with different currencies: {first.Currency.Code} and {second.Currency.Code}"));
        }

        return new Money(first.Amount + second.Amount, first.Currency);
    }

    /// <summary>
    /// Subtracts two Money values with the same currency
    /// </summary>
    /// <exception cref="DomainException">Thrown when currencies differ</exception>
    public static Money operator -(Money first, Money second)
    {
        if (first.Currency != second.Currency)
        {
            throw new DomainException(
                DomainErrors.General.InvalidInput($"Cannot subtract money with different currencies: {first.Currency.Code} and {second.Currency.Code}"));
        }

        return new Money(first.Amount - second.Amount, first.Currency);
    }

    /// <summary>
    /// Creates a zero Money value with no currency
    /// </summary>
    public static Money Zero() => new(0, Currency.None);

    /// <summary>
    /// Creates a zero Money value with the specified currency
    /// </summary>
    public static Money Zero(Currency currency) => new(0, currency);

    /// <summary>
    /// Checks if this Money value is zero
    /// </summary>
    public bool IsZero() => this == Zero(Currency);
}

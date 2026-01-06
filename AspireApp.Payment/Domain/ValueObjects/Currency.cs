using System;
using System.Collections.Generic;
using System.Linq;
using AspireApp.Domain.Shared.Common;

namespace AspireApp.Payment.Domain.ValueObjects;

/// <summary>
/// Currency value object representing a monetary currency
/// </summary>
public sealed record Currency
{
    internal static readonly Currency None = new("");
    public static readonly Currency Usd = new("USD");
    public static readonly Currency Eur = new("EUR");
    public static readonly Currency Aed = new("AED");
    public static readonly Currency Sar = new("SAR");

    private Currency(string code) => Code = code;

    /// <summary>
    /// ISO 4217 currency code
    /// </summary>
    public string Code { get; init; }

    /// <summary>
    /// Creates a Currency from a currency code
    /// </summary>
    /// <param name="code">ISO 4217 currency code</param>
    /// <returns>Currency instance</returns>
    /// <exception cref="DomainException">Thrown when currency code is invalid</exception>
    public static Currency FromCode(string code)
    {
        return All.FirstOrDefault(c => c.Code == code) ??
               throw new DomainException(DomainErrors.General.InvalidInput($"The currency code '{code}' is invalid"));
    }

    /// <summary>
    /// All supported currencies
    /// </summary>
    public static readonly IReadOnlyCollection<Currency> All =
    [
        Usd,
        Eur,
        Aed,
        Sar
    ];
}

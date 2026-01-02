using System.Text.RegularExpressions;

namespace AspireApp.Email.Domain.ValueObjects;

/// <summary>
/// Value object representing an email address with validation
/// </summary>
public record EmailAddress
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    public EmailAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Email address cannot be empty.", nameof(value));
        }

        if (!IsValidEmail(value))
        {
            throw new ArgumentException($"Invalid email address: {value}", nameof(value));
        }

        Value = value.Trim().ToLowerInvariant();
    }

    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        try
        {
            // Use both regex and MailAddress validation
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email && EmailRegex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }

    public static implicit operator string(EmailAddress emailAddress) => emailAddress.Value;

    public override string ToString() => Value;
}


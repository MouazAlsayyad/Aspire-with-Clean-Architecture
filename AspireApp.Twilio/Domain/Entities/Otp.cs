using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Entities;

namespace AspireApp.Twilio.Domain.Entities;

/// <summary>
/// OTP (One-Time Password) entity.
/// Represents an OTP code sent to a phone number for verification.
/// </summary>
public class Otp : BaseEntity
{
    public string PhoneNumber { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; }
    public DateTime? UsedAt { get; private set; }

    // Private constructor for EF Core
    private Otp() { }

    public Otp(
        string phoneNumber,
        string code,
        int expirationMinutes = 5)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number cannot be empty", nameof(phoneNumber));
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("OTP code cannot be empty", nameof(code));

        PhoneNumber = NormalizePhoneNumber(phoneNumber);
        Code = code;
        ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);
        IsUsed = false;
    }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    public bool IsValid => !IsUsed && !IsExpired;

    public void MarkAsUsed()
    {
        if (IsUsed)
            return;

        IsUsed = true;
        UsedAt = DateTime.UtcNow;
        SetLastModificationTime();
    }

    private static string NormalizePhoneNumber(string phoneNumber)
    {
        // Remove spaces and normalize phone number format
        return phoneNumber.Replace(" ", "").Trim();
    }
}


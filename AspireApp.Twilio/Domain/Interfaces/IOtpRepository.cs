using AspireApp.Domain.Shared.Entities;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Twilio.Domain.Entities;

namespace AspireApp.Twilio.Domain.Interfaces;

/// <summary>
/// Repository interface for Otp entity
/// </summary>
public interface IOtpRepository : IRepository<Otp>
{
    /// <summary>
    /// Gets the latest valid OTP for a phone number
    /// </summary>
    Task<Otp?> GetLatestValidOtpAsync(string phoneNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all OTPs for a phone number
    /// </summary>
    Task<List<Otp>> GetOtpsByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets expired OTPs
    /// </summary>
    Task<List<Otp>> GetExpiredOtpsAsync(CancellationToken cancellationToken = default);
}


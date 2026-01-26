using AspireApp.ApiService.Infrastructure.Repositories;
using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Twilio.Domain.Entities;
using AspireApp.Twilio.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AspireApp.Twilio.Infrastructure.Repositories;

public class CachedOtpRepository : CachedRepository<Otp>, IOtpRepository
{
    private readonly IOtpRepository _otpRepository;

    public CachedOtpRepository(
        IOtpRepository decorated,
        ICacheService cacheService,
        ILogger<CachedOtpRepository> logger)
        : base(decorated, cacheService, logger)
    {
        _otpRepository = decorated;
    }

    public async Task<Otp?> GetLatestValidOtpAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        // Cache key includes phone number. Expiry should be short (e.g. 5 mins).
        string key = $"otp:latest:{phoneNumber}";
        
        return await _cacheService.GetOrSetAsync(
            key,
            ct => _otpRepository.GetLatestValidOtpAsync(phoneNumber, ct),
            TimeSpanConstants.FiveMinutes,
            cancellationToken);
    }

    public async Task<List<Otp>> GetOtpsByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        // Usually list caching is harder to invalidate. We'll cache for a short time.
        string key = $"otp:list:{phoneNumber}";
        
        var result = await _cacheService.GetOrSetAsync(
            key,
            ct => _otpRepository.GetOtpsByPhoneNumberAsync(phoneNumber, ct),
            TimeSpanConstants.FiveMinutes,
            cancellationToken);

        return result ?? new List<Otp>();
    }

    public async Task<List<Otp>> GetExpiredOtpsAsync(CancellationToken cancellationToken = default)
    {
        // Do not cache this as it's likely used for cleanup jobs
        return await _otpRepository.GetExpiredOtpsAsync(cancellationToken);
    }
}

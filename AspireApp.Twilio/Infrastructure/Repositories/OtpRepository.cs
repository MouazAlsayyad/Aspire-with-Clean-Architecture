using AspireApp.ApiService.Infrastructure.Data;
using AspireApp.ApiService.Infrastructure.Repositories;
using AspireApp.Twilio.Domain.Entities;
using AspireApp.Twilio.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.Twilio.Infrastructure.Repositories;

public class OtpRepository : Repository<Otp>, IOtpRepository
{
    public OtpRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Otp?> GetLatestValidOtpAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        var normalizedPhone = phoneNumber.Replace(" ", "").Trim();
        return await _context.Set<Otp>()
            .Where(o => o.PhoneNumber == normalizedPhone && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow && !o.IsDeleted)
            .OrderByDescending(o => o.CreationTime)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Otp>> GetOtpsByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        var normalizedPhone = phoneNumber.Replace(" ", "").Trim();
        return await _context.Set<Otp>()
            .Where(o => o.PhoneNumber == normalizedPhone && !o.IsDeleted)
            .OrderByDescending(o => o.CreationTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Otp>> GetExpiredOtpsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<Otp>()
            .Where(o => o.ExpiresAt <= DateTime.UtcNow && !o.IsDeleted)
            .ToListAsync(cancellationToken);
    }
}


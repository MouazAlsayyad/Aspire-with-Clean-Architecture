using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Entities;
using User = AspireApp.ApiService.Domain.Users.Entities.User;

namespace AspireApp.ApiService.Domain.Auth.Entities;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; } = false;
    public DateTime? RevokedAt { get; private set; }

    // Navigation property
    public User? User { get; private set; }

    // Private constructor for EF Core
    private RefreshToken() { }

    public RefreshToken(Guid userId, string token, DateTime expiresAt)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty", nameof(userId));
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty", nameof(token));

        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
    }

    public void Revoke()
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        SetLastModificationTime();
    }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsValid => !IsRevoked && !IsExpired;
}


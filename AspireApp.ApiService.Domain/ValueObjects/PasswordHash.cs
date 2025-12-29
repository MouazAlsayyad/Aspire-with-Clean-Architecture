namespace AspireApp.ApiService.Domain.ValueObjects;

/// <summary>
/// Value object representing a password hash.
/// Value objects are immutable and compared by value, not reference.
/// </summary>
public class PasswordHash : IEquatable<PasswordHash>
{
    public string Hash { get; init; } = string.Empty;
    public string Salt { get; init; } = string.Empty;

    private PasswordHash() { }

    public PasswordHash(string hash, string salt)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new ArgumentException("Hash cannot be empty", nameof(hash));
        if (string.IsNullOrWhiteSpace(salt))
            throw new ArgumentException("Salt cannot be empty", nameof(salt));

        Hash = hash;
        Salt = salt;
    }

    public static PasswordHash Create(string hash, string salt) => new(hash, salt);

    /// <summary>
    /// Value objects are compared by their values, not by reference
    /// </summary>
    public bool Equals(PasswordHash? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Hash == other.Hash && Salt == other.Salt;
    }

    public override bool Equals(object? obj) => Equals(obj as PasswordHash);

    public override int GetHashCode() => HashCode.Combine(Hash, Salt);

    public static bool operator ==(PasswordHash? left, PasswordHash? right) => Equals(left, right);

    public static bool operator !=(PasswordHash? left, PasswordHash? right) => !Equals(left, right);
}


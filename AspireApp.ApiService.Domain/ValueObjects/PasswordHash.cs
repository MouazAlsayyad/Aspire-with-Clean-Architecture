namespace AspireApp.ApiService.Domain.ValueObjects;

public class PasswordHash
{
    public string Hash { get; private set; } = string.Empty;
    public string Salt { get; private set; } = string.Empty;

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
}


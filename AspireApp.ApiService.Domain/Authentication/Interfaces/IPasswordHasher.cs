namespace AspireApp.ApiService.Domain.Authentication.Interfaces;

/// <summary>
/// Domain interface for password hashing service.
/// This is a domain abstraction that will be implemented in Infrastructure layer.
/// </summary>
public interface IPasswordHasher
{
    (string Hash, string Salt) HashPassword(string password);
    bool VerifyPassword(string password, string hash, string salt);
}


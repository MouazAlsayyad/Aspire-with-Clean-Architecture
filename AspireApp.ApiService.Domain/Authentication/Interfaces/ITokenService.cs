using User = AspireApp.ApiService.Domain.Users.Entities.User;

namespace AspireApp.ApiService.Domain.Authentication.Interfaces;

/// <summary>
/// Domain interface for token generation service.
/// This is a domain abstraction that will be implemented in Infrastructure layer.
/// </summary>
public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    bool ValidateToken(string token);
    Guid? GetUserIdFromToken(string token);
}


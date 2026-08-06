using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Application.Interfaces.Services;

public record GeneratedAccessToken(string AccessToken, DateTime ExpiresAtUtc);

public interface ITokenService
{
    GeneratedAccessToken GenerateAccessToken(User user, IEnumerable<string> roles, string? activeRole);
    string GenerateRefreshTokenValue();
    string HashToken(string rawToken);
    TimeSpan RefreshTokenLifetime { get; }
}

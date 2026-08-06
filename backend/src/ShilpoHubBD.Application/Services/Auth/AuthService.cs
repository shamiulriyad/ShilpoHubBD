using Microsoft.Extensions.Configuration;
using ShilpoHubBD.Application.DTOs.Auth;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Application.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;

    private static readonly TimeSpan PasswordResetTokenLifetime = TimeSpan.FromHours(1);

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        ITokenService tokenService,
        IPasswordHasher passwordHasher,
        IEmailSender emailSender,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _emailSender = emailSender;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, string? ipAddress, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _userRepository.ExistsByEmailAsync(email, cancellationToken))
        {
            throw new ConflictException("Email is already registered.");
        }

        var requestedRoleNames = request.Roles.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var roles = await _roleRepository.GetByNamesAsync(requestedRoleNames, cancellationToken);
        if (roles.Count != requestedRoleNames.Count)
        {
            throw new NotFoundException("One or more requested roles do not exist.");
        }

        var now = DateTime.UtcNow;
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            FullName = request.FullName.Trim(),
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        };

        foreach (var role in roles)
        {
            user.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id,
                AssignedAt = now,
                AssignedByUserId = null,
            });
        }

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        var roleNames = roles.Select(r => r.Name).ToList();
        var activeRole = roleNames.Count == 1 ? roleNames[0] : null;
        return await IssueTokensAsync(user, roleNames, activeRole, ipAddress, cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailWithRolesAsync(email, cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("This account has been disabled.");
        }

        var roleNames = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var activeRole = roleNames.Count == 1 ? roleNames[0] : null;
        return await IssueTokensAsync(user, roleNames, activeRole, ipAddress, cancellationToken);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, string? ipAddress, CancellationToken cancellationToken)
    {
        var hash = _tokenService.HashToken(refreshToken);
        var stored = await _refreshTokenRepository.GetByTokenHashAsync(hash, cancellationToken);

        if (stored is null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        if (stored.IsRevoked)
        {
            // Reuse of an already-rotated/revoked token is a compromise signal.
            await _refreshTokenRepository.RevokeAllActiveForUserAsync(
                stored.UserId, ipAddress, "Reuse of a revoked refresh token was detected.", cancellationToken);
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        if (stored.IsExpired)
        {
            throw new UnauthorizedAccessException("Refresh token has expired.");
        }

        var user = await _userRepository.GetByIdWithRolesAsync(stored.UserId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        var newRawToken = _tokenService.GenerateRefreshTokenValue();
        var newHash = _tokenService.HashToken(newRawToken);

        stored.RevokedAt = DateTime.UtcNow;
        stored.RevokedByIp = ipAddress;
        stored.ReplacedByTokenHash = newHash;
        stored.ReasonRevoked = "Rotated on refresh.";

        var roleNames = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var activeRole = roleNames.Count == 1 ? roleNames[0] : null;
        var access = _tokenService.GenerateAccessToken(user, roleNames, activeRole);

        var newEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = newHash,
            ExpiresAt = DateTime.UtcNow.Add(_tokenService.RefreshTokenLifetime),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress,
        };
        await _refreshTokenRepository.AddAsync(newEntity, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            AccessToken = access.AccessToken,
            RefreshToken = newRawToken,
            ExpiresAtUtc = access.ExpiresAtUtc,
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Roles = roleNames,
            ActiveRole = activeRole,
        };
    }

    public async Task LogoutAsync(string refreshToken, string? ipAddress, CancellationToken cancellationToken)
    {
        var hash = _tokenService.HashToken(refreshToken);
        var stored = await _refreshTokenRepository.GetByTokenHashAsync(hash, cancellationToken);

        if (stored is not null && stored.IsActive)
        {
            stored.RevokedAt = DateTime.UtcNow;
            stored.RevokedByIp = ipAddress;
            stored.ReasonRevoked = "User logged out.";
            await _refreshTokenRepository.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailWithRolesAsync(email, cancellationToken);

        // Do not reveal whether the email exists.
        if (user is null)
        {
            return;
        }

        var rawToken = _tokenService.GenerateRefreshTokenValue();
        var hash = _tokenService.HashToken(rawToken);

        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = hash,
            ExpiresAt = DateTime.UtcNow.Add(PasswordResetTokenLifetime),
            CreatedAt = DateTime.UtcNow,
        };

        await _passwordResetTokenRepository.AddAsync(resetToken, cancellationToken);
        await _passwordResetTokenRepository.SaveChangesAsync(cancellationToken);

        var baseUrl = _configuration["Frontend:ResetPasswordUrl"] ?? "http://localhost:5173/reset-password";
        var resetLink = $"{baseUrl}?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(rawToken)}";

        await _emailSender.SendPasswordResetEmailAsync(user.Email, resetLink, cancellationToken);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailWithRolesAsync(email, cancellationToken);
        if (user is null)
        {
            throw new UnauthorizedAccessException("Invalid or expired reset token.");
        }

        var hash = _tokenService.HashToken(request.Token);
        var stored = await _passwordResetTokenRepository.GetActiveByTokenHashAsync(hash, user.Id, cancellationToken);
        if (stored is null)
        {
            throw new UnauthorizedAccessException("Invalid or expired reset token.");
        }

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        stored.UsedAt = DateTime.UtcNow;

        await _refreshTokenRepository.RevokeAllActiveForUserAsync(
            user.Id, null, "Password was reset.", cancellationToken);

        await _userRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<AuthResponse> SwitchRoleAsync(Guid userId, string role, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException("User not found.");
        }

        var roleNames = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        if (!roleNames.Contains(role, StringComparer.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("You do not hold the requested role.");
        }

        return await IssueTokensAsync(user, roleNames, role, ipAddress: null, cancellationToken);
    }

    private async Task<AuthResponse> IssueTokensAsync(
        User user, List<string> roleNames, string? activeRole, string? ipAddress, CancellationToken cancellationToken)
    {
        var access = _tokenService.GenerateAccessToken(user, roleNames, activeRole);
        var rawRefreshToken = _tokenService.GenerateRefreshTokenValue();
        var refreshHash = _tokenService.HashToken(rawRefreshToken);

        var refreshEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = refreshHash,
            ExpiresAt = DateTime.UtcNow.Add(_tokenService.RefreshTokenLifetime),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress,
        };

        await _refreshTokenRepository.AddAsync(refreshEntity, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            AccessToken = access.AccessToken,
            RefreshToken = rawRefreshToken,
            ExpiresAtUtc = access.ExpiresAtUtc,
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Roles = roleNames,
            ActiveRole = activeRole,
        };
    }
}

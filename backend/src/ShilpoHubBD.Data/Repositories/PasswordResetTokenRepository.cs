using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Data.Repositories;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly ShilpoHubDbContext _context;

    public PasswordResetTokenRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public Task<PasswordResetToken?> GetActiveByTokenHashAsync(string tokenHash, Guid userId, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        return _context.PasswordResetTokens.FirstOrDefaultAsync(
            t => t.TokenHash == tokenHash && t.UserId == userId && t.UsedAt == null && t.ExpiresAt > now,
            cancellationToken);
    }

    public async Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken)
        => await _context.PasswordResetTokens.AddAsync(token, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

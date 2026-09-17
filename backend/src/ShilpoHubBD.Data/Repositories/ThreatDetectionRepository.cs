using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Security;

namespace ShilpoHubBD.Data.Repositories;

public class ThreatDetectionRepository : IThreatDetectionRepository
{
    private readonly ShilpoHubDbContext _context;

    public ThreatDetectionRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public async Task AddLoginAttemptAsync(LoginAttempt attempt, CancellationToken cancellationToken)
        => await _context.LoginAttempts.AddAsync(attempt, cancellationToken);

    public Task<bool> IsIpBlockedAsync(string ipAddress, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        return _context.BlockedIpAddresses.AnyAsync(
            b => b.IpAddress == ipAddress && (b.ExpiresAt == null || b.ExpiresAt > now), cancellationToken);
    }

    public async Task<(List<LoginAttempt> Items, int TotalCount)> GetFailedLoginsPagedAsync(
        int page, int pageSize, CancellationToken cancellationToken)
    {
        var attempts = _context.LoginAttempts.Where(a => !a.Succeeded).OrderByDescending(a => a.CreatedAt);

        var totalCount = await attempts.CountAsync(cancellationToken);
        var items = await attempts.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<List<(string IpAddress, int FailedCount)>> GetSuspiciousIpsAsync(
        DateTime since, int minFailedAttempts, CancellationToken cancellationToken)
    {
        var rows = await _context.LoginAttempts
            .Where(a => !a.Succeeded && a.CreatedAt >= since && a.IpAddress != null)
            .GroupBy(a => a.IpAddress!)
            .Select(g => new { IpAddress = g.Key, Count = g.Count() })
            .Where(x => x.Count >= minFailedAttempts)
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        var blockedIps = await _context.BlockedIpAddresses
            .Where(b => b.ExpiresAt == null || b.ExpiresAt > DateTime.UtcNow)
            .Select(b => b.IpAddress)
            .ToListAsync(cancellationToken);

        return rows
            .Where(r => !blockedIps.Contains(r.IpAddress))
            .Select(r => (r.IpAddress, r.Count))
            .ToList();
    }

    public Task<List<BlockedIpAddress>> GetBlockedIpsAsync(CancellationToken cancellationToken)
        => _context.BlockedIpAddresses
            .Include(b => b.BlockedBy)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<BlockedIpAddress?> GetBlockedIpByAddressAsync(string ipAddress, CancellationToken cancellationToken)
        => _context.BlockedIpAddresses.FirstOrDefaultAsync(b => b.IpAddress == ipAddress, cancellationToken);

    public async Task AddBlockedIpAsync(BlockedIpAddress blocked, CancellationToken cancellationToken)
        => await _context.BlockedIpAddresses.AddAsync(blocked, cancellationToken);

    public void RemoveBlockedIp(BlockedIpAddress blocked)
        => _context.BlockedIpAddresses.Remove(blocked);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

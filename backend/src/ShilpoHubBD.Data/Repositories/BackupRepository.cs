using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Security;

namespace ShilpoHubBD.Data.Repositories;

public class BackupRepository : IBackupRepository
{
    private readonly ShilpoHubDbContext _context;

    public BackupRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<BackupRecord> WithDetails()
        => _context.BackupRecords.Include(b => b.RequestedBy);

    public async Task AddAsync(BackupRecord record, CancellationToken cancellationToken)
        => await _context.BackupRecords.AddAsync(record, cancellationToken);

    public Task<BackupRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public async Task<(List<BackupRecord> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, CancellationToken cancellationToken)
    {
        var records = WithDetails().OrderByDescending(b => b.StartedAt);

        var totalCount = await records.CountAsync(cancellationToken);
        var items = await records.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public void Remove(BackupRecord record)
        => _context.BackupRecords.Remove(record);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

using ShilpoHubBD.Domain.Entities.Security;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IBackupRepository
{
    Task AddAsync(BackupRecord record, CancellationToken cancellationToken);
    Task<BackupRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<(List<BackupRecord> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken);
    void Remove(BackupRecord record);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

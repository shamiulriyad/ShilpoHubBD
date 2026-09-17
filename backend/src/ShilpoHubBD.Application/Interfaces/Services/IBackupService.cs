using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Security;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IBackupService
{
    Task<BackupRecordDto> TriggerBackupAsync(Guid userId, CancellationToken cancellationToken);
    Task<PagedResult<BackupRecordDto>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<BackupRecordDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

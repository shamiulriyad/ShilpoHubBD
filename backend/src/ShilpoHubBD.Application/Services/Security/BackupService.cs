using Microsoft.Extensions.Configuration;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Security;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Security;

namespace ShilpoHubBD.Application.Services.Security;

public class BackupService : IBackupService
{
    private readonly IBackupRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly IBackupRunner _backupRunner;
    private readonly string _backupDirectory;

    public BackupService(
        IBackupRepository repository, IUserRepository userRepository, IBackupRunner backupRunner, IConfiguration configuration)
    {
        _repository = repository;
        _userRepository = userRepository;
        _backupRunner = backupRunner;
        _backupDirectory = configuration["Backup:Directory"]
            ?? Path.Combine(AppContext.BaseDirectory, "backups");
    }

    public async Task<BackupRecordDto> TriggerBackupAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        Directory.CreateDirectory(_backupDirectory);
        var fileName = $"backup-{DateTime.UtcNow:yyyyMMdd-HHmmss}.sql";
        var filePath = Path.Combine(_backupDirectory, fileName);

        var record = new BackupRecord
        {
            Id = Guid.NewGuid(),
            RequestedByUserId = userId,
            RequestedBy = user,
            Status = BackupStatus.Running,
            StartedAt = DateTime.UtcNow,
        };

        await _repository.AddAsync(record, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var result = await _backupRunner.RunAsync(filePath, cancellationToken);

        record.Status = result.Success ? BackupStatus.Completed : BackupStatus.Failed;
        record.FilePath = result.Success ? filePath : null;
        record.FileSizeBytes = result.FileSizeBytes;
        record.ErrorMessage = result.ErrorMessage;
        record.CompletedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        return ToDto(record);
    }

    public async Task<PagedResult<BackupRecordDto>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var (items, totalCount) = await _repository.GetPagedAsync(page, pageSize, cancellationToken);
        return new PagedResult<BackupRecordDto>
        {
            Items = items.Select(ToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<BackupRecordDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var record = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Backup record not found.");
        return ToDto(record);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var record = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Backup record not found.");

        if (!string.IsNullOrEmpty(record.FilePath) && File.Exists(record.FilePath))
        {
            File.Delete(record.FilePath);
        }

        _repository.Remove(record);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private static BackupRecordDto ToDto(BackupRecord record) => new()
    {
        Id = record.Id,
        RequestedByName = record.RequestedBy.FullName,
        Status = record.Status,
        FilePath = record.FilePath,
        FileSizeBytes = record.FileSizeBytes,
        ErrorMessage = record.ErrorMessage,
        StartedAt = record.StartedAt,
        CompletedAt = record.CompletedAt,
    };
}

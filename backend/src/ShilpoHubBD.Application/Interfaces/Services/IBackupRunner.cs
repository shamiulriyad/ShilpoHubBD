namespace ShilpoHubBD.Application.Interfaces.Services;

public record BackupRunResult(bool Success, long? FileSizeBytes, string? ErrorMessage);

/// <summary>Runs a database backup (via <c>pg_dump</c>) to a file on disk. Best-effort: the target
/// environment must have PostgreSQL client tools available on PATH, or configure "Backup:PgDumpPath".</summary>
public interface IBackupRunner
{
    Task<BackupRunResult> RunAsync(string outputFilePath, CancellationToken cancellationToken);
}

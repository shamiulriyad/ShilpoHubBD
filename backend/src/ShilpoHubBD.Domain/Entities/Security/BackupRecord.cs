using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Security;

/// <summary>A database backup job triggered from the admin dashboard, via <c>pg_dump</c>. Metadata only —
/// the dump file itself lives on disk at <see cref="FilePath"/>.</summary>
public class BackupRecord
{
    public Guid Id { get; set; }

    public Guid RequestedByUserId { get; set; }
    public User RequestedBy { get; set; } = null!;

    public BackupStatus Status { get; set; } = BackupStatus.Running;
    public string? FilePath { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? ErrorMessage { get; set; }

    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

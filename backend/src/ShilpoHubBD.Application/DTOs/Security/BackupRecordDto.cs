using ShilpoHubBD.Domain.Entities.Security;

namespace ShilpoHubBD.Application.DTOs.Security;

public class BackupRecordDto
{
    public Guid Id { get; set; }
    public string RequestedByName { get; set; } = string.Empty;
    public BackupStatus Status { get; set; }
    public string? FilePath { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

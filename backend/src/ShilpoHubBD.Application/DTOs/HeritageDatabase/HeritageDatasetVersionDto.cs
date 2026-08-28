namespace ShilpoHubBD.Application.DTOs.HeritageDatabase;

public class HeritageDatasetVersionDto
{
    public Guid Id { get; set; }
    public Guid HeritageDatasetId { get; set; }
    public int VersionNumber { get; set; }
    public string? Label { get; set; }
    public string Changelog { get; set; } = string.Empty;
    public int RecordCount { get; set; }
    public string Format { get; set; } = string.Empty;
    public string? SourceFileName { get; set; }
    public string? SourceFileUrl { get; set; }
    public string? SourceContentHash { get; set; }
    public int? ImportedRowCount { get; set; }
    public int ImportErrorCount { get; set; }
    public string? ImportNotes { get; set; }
    public string? SchemaJson { get; set; }
    public bool IsCurrent { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
}

namespace ShilpoHubBD.Application.DTOs.HeritageDatabase;

public class HeritageDatasetExportDto
{
    public Guid Id { get; set; }
    public Guid HeritageDatasetId { get; set; }
    public string DatasetName { get; set; } = string.Empty;
    public Guid? DatasetVersionId { get; set; }
    public int? VersionNumber { get; set; }
    public Guid RequestedByUserId { get; set; }
    public string RequestedByName { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public int RowCount { get; set; }
    public string? FilterJson { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

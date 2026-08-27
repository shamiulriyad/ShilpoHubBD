namespace ShilpoHubBD.Application.DTOs.HeritageDatabase;

public class CreateHeritageDatasetVersionRequest
{
    public string? Label { get; set; }
    public string Changelog { get; set; } = string.Empty;
    public int? RecordCount { get; set; }
    public string? Format { get; set; }
    public string? SourceFileName { get; set; }
    public string? SourceFileUrl { get; set; }
    public string? SourceContentHash { get; set; }
    public int? ImportedRowCount { get; set; }
    public int ImportErrorCount { get; set; }
    public string? ImportNotes { get; set; }
    public string? SchemaJson { get; set; }
    public bool Publish { get; set; } = true;
    public bool SetAsCurrent { get; set; } = true;
}

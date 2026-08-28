namespace ShilpoHubBD.Application.DTOs.HeritageDatabase;

public class CreateHeritageDatasetExportRequest
{
    public Guid? DatasetVersionId { get; set; }
    public string? Format { get; set; }
    public string? FilterJson { get; set; }
    public string? Notes { get; set; }
}

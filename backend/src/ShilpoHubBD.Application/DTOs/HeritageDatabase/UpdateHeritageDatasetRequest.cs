namespace ShilpoHubBD.Application.DTOs.HeritageDatabase;

public class UpdateHeritageDatasetRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string AccessLevel { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public string? SourceOrganization { get; set; }
    public string? SourceReference { get; set; }
    public string? License { get; set; }
    public string? Tags { get; set; }
    public bool IsLive { get; set; }
}

namespace ShilpoHubBD.Application.DTOs.HeritageDatabase;

public class CreateHeritageDatasetRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? AccessLevel { get; set; }
    public string? SourceType { get; set; }
    public string? SourceOrganization { get; set; }
    public string? SourceReference { get; set; }
    public string? License { get; set; }
    public string? Tags { get; set; }
    public bool IsLive { get; set; } = true;
}

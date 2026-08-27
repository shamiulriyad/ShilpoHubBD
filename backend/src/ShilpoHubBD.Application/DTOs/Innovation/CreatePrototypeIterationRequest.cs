namespace ShilpoHubBD.Application.DTOs.Innovation;

public class CreatePrototypeIterationRequest
{
    public string? Label { get; set; }
    public string ChangeSummary { get; set; } = string.Empty;
    public string? ArtifactUrl { get; set; }
    public bool SetAsCurrent { get; set; } = true;
}

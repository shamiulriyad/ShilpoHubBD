namespace ShilpoHubBD.Application.DTOs.Innovation;

public class CreateExperimentVersionRequest
{
    public string? Label { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string ConfigJson { get; set; } = string.Empty;
    public string? Framework { get; set; }
    public string? ArtifactUrl { get; set; }
    public bool SetAsCurrent { get; set; } = true;
}

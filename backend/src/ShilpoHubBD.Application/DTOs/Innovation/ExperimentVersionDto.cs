namespace ShilpoHubBD.Application.DTOs.Innovation;

public class ExperimentVersionDto
{
    public Guid Id { get; set; }
    public Guid InnovationExperimentId { get; set; }
    public int VersionNumber { get; set; }
    public string? Label { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string ConfigJson { get; set; } = string.Empty;
    public string? Framework { get; set; }
    public string? ArtifactUrl { get; set; }
    public bool IsCurrent { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

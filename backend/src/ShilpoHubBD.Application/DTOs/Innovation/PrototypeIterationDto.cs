namespace ShilpoHubBD.Application.DTOs.Innovation;

public class PrototypeIterationDto
{
    public Guid Id { get; set; }
    public Guid InnovationPrototypeId { get; set; }
    public int VersionNumber { get; set; }
    public string? Label { get; set; }
    public string ChangeSummary { get; set; } = string.Empty;
    public string? ArtifactUrl { get; set; }
    public bool IsCurrent { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

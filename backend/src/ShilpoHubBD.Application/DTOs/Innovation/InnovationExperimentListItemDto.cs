namespace ShilpoHubBD.Application.DTOs.Innovation;

public class InnovationExperimentListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ModelType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Framework { get; set; }
    public Guid OwnerUserId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public Guid? ResearchProjectId { get; set; }
    public Guid? HeritageDatasetId { get; set; }
    public int VersionCount { get; set; }
    public int RunCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

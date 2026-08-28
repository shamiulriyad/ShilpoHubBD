namespace ShilpoHubBD.Application.DTOs.Innovation;

public class CreateInnovationExperimentRequest
{
    public string Name { get; set; } = string.Empty;
    public string Objective { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ModelType { get; set; } = string.Empty;
    public string? Framework { get; set; }
    public string? ConfigJson { get; set; }
    public Guid? ResearchProjectId { get; set; }
    public Guid? HeritageDatasetId { get; set; }
}

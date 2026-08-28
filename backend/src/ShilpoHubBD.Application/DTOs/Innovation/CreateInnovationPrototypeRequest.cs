namespace ShilpoHubBD.Application.DTOs.Innovation;

public class CreateInnovationPrototypeRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Category { get; set; }
    public Guid? ResearchProjectId { get; set; }
    public Guid? PreservationStrategyId { get; set; }
    public Guid? InnovationExperimentId { get; set; }
}

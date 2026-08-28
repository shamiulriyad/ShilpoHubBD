namespace ShilpoHubBD.Application.DTOs.Innovation;

public class UpdateInnovationPrototypeRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? ResearchProjectId { get; set; }
    public Guid? PreservationStrategyId { get; set; }
    public Guid? InnovationExperimentId { get; set; }
}

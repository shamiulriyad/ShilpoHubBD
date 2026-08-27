namespace ShilpoHubBD.Application.DTOs.Innovation;

public class UpdateHeritageInnovationSubmissionRequest
{
    public string Title { get; set; } = string.Empty;
    public string Problem { get; set; } = string.Empty;
    public string Solution { get; set; } = string.Empty;
    public string? ResearchEvidence { get; set; }
    public Guid? ResearchProjectId { get; set; }
    public Guid? InnovationPrototypeId { get; set; }
    public Guid? PreservationStrategyId { get; set; }
    public Guid? HeritageDatasetId { get; set; }
}

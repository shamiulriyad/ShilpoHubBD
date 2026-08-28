namespace ShilpoHubBD.Application.DTOs.Innovation;

public class CreatePreservationStrategyRequest
{
    public string Title { get; set; } = string.Empty;
    public string HeritageProblem { get; set; } = string.Empty;
    public string ProposedSolution { get; set; } = string.Empty;
    public string? ExpectedImpact { get; set; }
    public string? EvidenceReferences { get; set; }
    public Guid? ResearchProjectId { get; set; }
    public Guid? HeritageDatasetId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? TargetDate { get; set; }
}

using System.Collections.Generic;

namespace ShilpoHubBD.Application.DTOs.Innovation;

public class PreservationStrategyDetailDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string HeritageProblem { get; set; } = string.Empty;
    public string ProposedSolution { get; set; } = string.Empty;
    public string? ExpectedImpact { get; set; }
    public string? EvidenceReferences { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid OwnerUserId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public Guid? ResearchProjectId { get; set; }
    public Guid? HeritageDatasetId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? TargetDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<StrategyObjectiveDto> Objectives { get; set; } = new();
    public List<StrategyActionDto> Actions { get; set; } = new();
}

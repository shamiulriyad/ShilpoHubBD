using System.Collections.Generic;

namespace ShilpoHubBD.Application.DTOs.Innovation;

public class InnovationPrototypeDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid OwnerUserId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public Guid? ResearchProjectId { get; set; }
    public Guid? PreservationStrategyId { get; set; }
    public Guid? InnovationExperimentId { get; set; }
    public Guid? CurrentIterationId { get; set; }
    public int VersionCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<PrototypeIterationDto> Iterations { get; set; } = new();
    public List<PrototypeTestCaseDto> TestCases { get; set; } = new();
    public int TestRunCount { get; set; }
    public int OpenIssueCount { get; set; }
}

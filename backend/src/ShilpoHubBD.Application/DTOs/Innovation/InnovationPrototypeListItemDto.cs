namespace ShilpoHubBD.Application.DTOs.Innovation;

public class InnovationPrototypeListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Category { get; set; }
    public Guid OwnerUserId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public Guid? ResearchProjectId { get; set; }
    public Guid? PreservationStrategyId { get; set; }
    public Guid? InnovationExperimentId { get; set; }
    public int VersionCount { get; set; }
    public int TestCaseCount { get; set; }
    public int TestRunCount { get; set; }
    public int OpenIssueCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

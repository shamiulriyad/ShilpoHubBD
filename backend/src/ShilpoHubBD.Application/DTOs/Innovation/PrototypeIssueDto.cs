namespace ShilpoHubBD.Application.DTOs.Innovation;

public class PrototypeIssueDto
{
    public Guid Id { get; set; }
    public Guid InnovationPrototypeId { get; set; }
    public Guid? PrototypeTestRunId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid ReportedByUserId { get; set; }
    public string ReportedByName { get; set; } = string.Empty;
    public Guid? ResolvedByUserId { get; set; }
    public string? ResolvedByName { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? Resolution { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

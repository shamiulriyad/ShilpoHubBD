namespace ShilpoHubBD.Application.DTOs.Innovation;

public class CreatePrototypeIssueRequest
{
    public Guid? PrototypeTestRunId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Severity { get; set; }
}

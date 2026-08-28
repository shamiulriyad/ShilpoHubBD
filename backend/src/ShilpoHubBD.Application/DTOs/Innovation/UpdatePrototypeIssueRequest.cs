namespace ShilpoHubBD.Application.DTOs.Innovation;

public class UpdatePrototypeIssueRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Resolution { get; set; }
}

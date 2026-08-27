namespace ShilpoHubBD.Application.DTOs.Innovation;

public class PrototypeTestResultDto
{
    public Guid Id { get; set; }
    public Guid? PrototypeTestCaseId { get; set; }
    public string CaseTitle { get; set; } = string.Empty;
    public string Outcome { get; set; } = string.Empty;
    public string? ActualResult { get; set; }
    public string? Notes { get; set; }
}

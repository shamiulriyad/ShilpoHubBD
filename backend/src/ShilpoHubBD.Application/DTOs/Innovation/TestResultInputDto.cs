namespace ShilpoHubBD.Application.DTOs.Innovation;

public class TestResultInputDto
{
    public Guid? PrototypeTestCaseId { get; set; }
    public string? CaseTitle { get; set; }
    public string Outcome { get; set; } = string.Empty;
    public string? ActualResult { get; set; }
    public string? Notes { get; set; }
}

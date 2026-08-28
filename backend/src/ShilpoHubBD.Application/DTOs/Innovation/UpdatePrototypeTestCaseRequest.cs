namespace ShilpoHubBD.Application.DTOs.Innovation;

public class UpdatePrototypeTestCaseRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Steps { get; set; }
    public string ExpectedResult { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public bool IsActive { get; set; }
}

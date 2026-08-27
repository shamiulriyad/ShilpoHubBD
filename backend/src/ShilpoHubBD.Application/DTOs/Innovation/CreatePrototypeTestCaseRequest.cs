namespace ShilpoHubBD.Application.DTOs.Innovation;

public class CreatePrototypeTestCaseRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Steps { get; set; }
    public string ExpectedResult { get; set; } = string.Empty;
    public string? Priority { get; set; }
    public int OrderIndex { get; set; }
}

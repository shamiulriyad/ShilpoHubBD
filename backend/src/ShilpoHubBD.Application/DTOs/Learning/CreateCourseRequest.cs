namespace ShilpoHubBD.Application.DTOs.Learning;

public class CreateCourseRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int? MaxApprentices { get; set; }
}

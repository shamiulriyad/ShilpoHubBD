namespace ShilpoHubBD.Application.DTOs.Learning;

public class UpdateCourseModuleRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

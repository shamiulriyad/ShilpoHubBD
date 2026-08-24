namespace ShilpoHubBD.Application.DTOs.Learning;

public class CreateLessonRequest
{
    public Guid? ModuleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? VideoUrl { get; set; }
    public int DisplayOrder { get; set; }
}

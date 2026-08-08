namespace ShilpoHubBD.Application.DTOs.Learning;

public class CreateLessonRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? VideoUrl { get; set; }
    public int DisplayOrder { get; set; }
}

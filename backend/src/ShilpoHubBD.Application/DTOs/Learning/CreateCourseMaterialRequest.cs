namespace ShilpoHubBD.Application.DTOs.Learning;

public class CreateCourseMaterialRequest
{
    public Guid? LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

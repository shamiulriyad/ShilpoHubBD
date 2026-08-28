namespace ShilpoHubBD.Application.DTOs.Learning;

public class MarkLessonProgressRequest
{
    public Guid LessonId { get; set; }
    public bool IsCompleted { get; set; } = true;
}

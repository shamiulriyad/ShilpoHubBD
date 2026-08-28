namespace ShilpoHubBD.Application.DTOs.Learning;

public class LessonProgressDto
{
    public Guid LessonId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
}

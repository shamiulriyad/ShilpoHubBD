namespace ShilpoHubBD.Domain.Entities.Learning;

public class CourseMaterial
{
    public Guid Id { get; set; }

    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public Guid? LessonId { get; set; }
    public CourseLesson? Lesson { get; set; }

    public string Title { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}

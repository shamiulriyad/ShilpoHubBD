namespace ShilpoHubBD.Domain.Entities.Learning;

public class LessonProgress
{
    public Guid Id { get; set; }

    public Guid EnrollmentId { get; set; }
    public CourseEnrollment Enrollment { get; set; } = null!;

    public Guid LessonId { get; set; }
    public CourseLesson Lesson { get; set; } = null!;

    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
}

namespace ShilpoHubBD.Domain.Entities.Learning;

public class Course
{
    public Guid Id { get; set; }

    public Guid MentorId { get; set; }
    public MentorProfile Mentor { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    public CourseStatus Status { get; set; } = CourseStatus.Draft;
    public int? MaxApprentices { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }

    public ICollection<CourseLesson> Lessons { get; set; } = new List<CourseLesson>();
    public ICollection<CourseEnrollment> Enrollments { get; set; } = new List<CourseEnrollment>();
}

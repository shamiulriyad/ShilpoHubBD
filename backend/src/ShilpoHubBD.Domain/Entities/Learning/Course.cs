namespace ShilpoHubBD.Domain.Entities.Learning;

public class Course
{
    public Guid Id { get; set; }

    // Exactly one of MentorId/TrainerProfileId is set, identifying the course author.
    public Guid? MentorId { get; set; }
    public MentorProfile? Mentor { get; set; }

    public Guid? TrainerProfileId { get; set; }
    public AcademyMemberProfile? TrainerProfile { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    public Guid? CategoryId { get; set; }
    public CourseCategory? CourseCategory { get; set; }

    public CourseStatus Status { get; set; } = CourseStatus.Draft;
    public int? MaxApprentices { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }

    public ICollection<CourseLesson> Lessons { get; set; } = new List<CourseLesson>();
    public ICollection<CourseEnrollment> Enrollments { get; set; } = new List<CourseEnrollment>();
    public ICollection<CourseModule> Modules { get; set; } = new List<CourseModule>();
    public ICollection<CourseMaterial> Materials { get; set; } = new List<CourseMaterial>();
}

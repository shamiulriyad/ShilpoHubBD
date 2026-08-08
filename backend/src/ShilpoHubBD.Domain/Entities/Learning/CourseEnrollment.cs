using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Learning;

public class CourseEnrollment
{
    public Guid Id { get; set; }

    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public Guid ApprenticeId { get; set; }
    public User Apprentice { get; set; } = null!;

    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;

    public DateTime EnrolledAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public ICollection<LessonProgress> LessonProgress { get; set; } = new List<LessonProgress>();
}

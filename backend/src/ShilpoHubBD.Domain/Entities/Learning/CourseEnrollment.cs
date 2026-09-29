using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Learning;

public class CourseEnrollment
{
    public string AttendanceMode { get; set; } = "Online";
    public decimal FeeAmount { get; set; }
    public string PaymentStatus { get; set; } = "Free";
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

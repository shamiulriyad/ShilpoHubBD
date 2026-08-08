namespace ShilpoHubBD.Domain.Entities.Learning;

public class TrainingCertificate
{
    public Guid Id { get; set; }

    public Guid EnrollmentId { get; set; }
    public CourseEnrollment Enrollment { get; set; } = null!;

    public string CertificateNumber { get; set; } = string.Empty;

    // Snapshotted at issuance so the certificate stays accurate even if the course/mentor/apprentice
    // records are edited later.
    public string CourseTitle { get; set; } = string.Empty;
    public string ApprenticeName { get; set; } = string.Empty;
    public string MentorName { get; set; } = string.Empty;

    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }

    public DateTime IssuedAt { get; set; }
}

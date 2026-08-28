using ShilpoHubBD.Domain.Entities.Apprenticeship;
using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Learning;

public class TrainingCertificate
{
    public Guid Id { get; set; }

    public CertificateType Type { get; set; } = CertificateType.Course;

    public Guid RecipientUserId { get; set; }
    public User Recipient { get; set; } = null!;

    public Guid? IssuerUserId { get; set; }
    public User? Issuer { get; set; }

    // Exactly one of EnrollmentId/ApprenticeEnrollmentId/HeritageSkillId is set, matching Type.
    public Guid? EnrollmentId { get; set; }
    public CourseEnrollment? Enrollment { get; set; }

    public Guid? ApprenticeEnrollmentId { get; set; }
    public ApprenticeEnrollment? ApprenticeEnrollment { get; set; }

    public Guid? HeritageSkillId { get; set; }
    public HeritageSkill? HeritageSkill { get; set; }

    public string CertificateNumber { get; set; } = string.Empty;

    // Snapshotted at issuance so the certificate stays accurate even if the underlying records are edited later.
    public string Title { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string IssuerName { get; set; } = string.Empty;

    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }

    public DateTime IssuedAt { get; set; }
}

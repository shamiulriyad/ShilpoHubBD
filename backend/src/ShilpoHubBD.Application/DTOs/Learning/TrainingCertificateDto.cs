namespace ShilpoHubBD.Application.DTOs.Learning;

public class TrainingCertificateDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public Guid RecipientUserId { get; set; }
    public Guid? EnrollmentId { get; set; }
    public Guid? ApprenticeEnrollmentId { get; set; }
    public Guid? HeritageSkillId { get; set; }
    public string CertificateNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string IssuerName { get; set; } = string.Empty;
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }
    public DateTime IssuedAt { get; set; }
}

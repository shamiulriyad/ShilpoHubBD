namespace ShilpoHubBD.Application.DTOs.Learning;

public class TrainingCertificateDto
{
    public Guid Id { get; set; }
    public Guid EnrollmentId { get; set; }
    public string CertificateNumber { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public string ApprenticeName { get; set; } = string.Empty;
    public string MentorName { get; set; } = string.Empty;
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }
    public DateTime IssuedAt { get; set; }
}

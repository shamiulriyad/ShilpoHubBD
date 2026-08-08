namespace ShilpoHubBD.Application.DTOs.Learning;

public class TrainingCertificateVerificationResultDto
{
    public bool IsValid { get; set; }
    public string CertificateNumber { get; set; } = string.Empty;
    public string? CourseTitle { get; set; }
    public string? ApprenticeName { get; set; }
    public string? MentorName { get; set; }
    public DateTime? IssuedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}

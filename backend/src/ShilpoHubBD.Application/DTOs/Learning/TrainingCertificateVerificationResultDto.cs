namespace ShilpoHubBD.Application.DTOs.Learning;

public class TrainingCertificateVerificationResultDto
{
    public bool IsValid { get; set; }
    public string CertificateNumber { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? Title { get; set; }
    public string? RecipientName { get; set; }
    public string? IssuerName { get; set; }
    public DateTime? IssuedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}

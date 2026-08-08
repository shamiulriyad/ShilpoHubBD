namespace ShilpoHubBD.Application.DTOs.Certificate;

public class CertificateVerificationResultDto
{
    public bool IsValid { get; set; }
    public string CertificateNumber { get; set; } = string.Empty;
    public string? ProductName { get; set; }
    public string? ProducerName { get; set; }
    public string? District { get; set; }
    public string? Category { get; set; }
    public DateTime? IssuedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}

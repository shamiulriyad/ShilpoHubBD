namespace ShilpoHubBD.Application.DTOs.Certificate;

public class CertificateDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CertificateNumber { get; set; } = string.Empty;
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }
    public DateTime IssuedAt { get; set; }
}

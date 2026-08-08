namespace ShilpoHubBD.Application.DTOs.Sustainability;

public class SustainableMaterialCertificationDto
{
    public Guid Id { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public string CertifyingBody { get; set; } = string.Empty;
    public string CertificateReference { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
}

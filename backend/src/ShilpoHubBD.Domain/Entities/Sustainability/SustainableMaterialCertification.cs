namespace ShilpoHubBD.Domain.Entities.Sustainability;

public class SustainableMaterialCertification
{
    public Guid Id { get; set; }

    public Guid SustainabilityProfileId { get; set; }
    public SustainabilityProfile SustainabilityProfile { get; set; } = null!;

    public string MaterialName { get; set; } = string.Empty;
    public string CertifyingBody { get; set; } = string.Empty;
    public string CertificateReference { get; set; } = string.Empty;

    public DateTime IssuedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsVerified { get; set; }

    public DateTime CreatedAt { get; set; }
}

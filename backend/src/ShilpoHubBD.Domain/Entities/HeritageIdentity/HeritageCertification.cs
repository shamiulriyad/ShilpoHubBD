namespace ShilpoHubBD.Domain.Entities.HeritageIdentity;

public class HeritageCertification
{
    public Guid Id { get; set; }

    public Guid ProducerHeritageIdentityId { get; set; }
    public ProducerHeritageIdentity ProducerHeritageIdentity { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string IssuingBody { get; set; } = string.Empty;
    public int IssuedYear { get; set; }
    public int? ExpiryYear { get; set; }
    public string? CertificateNumber { get; set; }
    public string? CertificateUrl { get; set; }
    public int DisplayOrder { get; set; }
}

using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.Certificate;

public class Certificate
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public Guid ProducerId { get; set; }
    public User Producer { get; set; } = null!;

    public string CertificateNumber { get; set; } = string.Empty;

    // Snapshotted at issuance so the certificate stays accurate even if the
    // underlying product/producer records are edited later.
    public string ProductName { get; set; } = string.Empty;
    public string ProducerName { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }

    public DateTime IssuedAt { get; set; }
}

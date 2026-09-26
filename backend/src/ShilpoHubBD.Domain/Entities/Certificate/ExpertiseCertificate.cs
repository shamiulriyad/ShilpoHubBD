using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Certificate;

/// <summary>Issued by an admin to a producer whose customer ratings reach a level.</summary>
public class ExpertiseCertificate
{
    public Guid Id { get; set; }

    public Guid ProducerId { get; set; }
    public User Producer { get; set; } = null!;

    public string CertificateNumber { get; set; } = string.Empty;
    public ExpertiseLevel Level { get; set; }

    // Snapshot at issue time.
    public string Expertise { get; set; } = string.Empty;
    public decimal AverageRating { get; set; }
    public int RatingCount { get; set; }

    public Guid IssuedByUserId { get; set; }
    public User IssuedBy { get; set; } = null!;
    public DateTime IssuedAt { get; set; }

    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }
}

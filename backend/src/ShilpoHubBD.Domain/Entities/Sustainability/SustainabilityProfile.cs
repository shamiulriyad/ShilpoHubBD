using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Sustainability;

public class SustainabilityProfile
{
    public Guid Id { get; set; }

    public Guid ProducerId { get; set; }
    public User Producer { get; set; } = null!;

    public decimal EcoScore { get; set; }
    public GreenBadgeLevel BadgeLevel { get; set; } = GreenBadgeLevel.None;
    public decimal TotalCarbonSavingsKg { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastCalculatedAt { get; set; }

    public ICollection<SustainableMaterialRecord> MaterialRecords { get; set; } = new List<SustainableMaterialRecord>();
    public ICollection<SustainableMaterialCertification> Certifications { get; set; } = new List<SustainableMaterialCertification>();
}

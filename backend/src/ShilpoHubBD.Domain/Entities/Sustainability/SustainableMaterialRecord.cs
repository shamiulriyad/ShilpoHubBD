using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.Sustainability;

public class SustainableMaterialRecord
{
    public Guid Id { get; set; }

    public Guid SustainabilityProfileId { get; set; }
    public SustainabilityProfile SustainabilityProfile { get; set; } = null!;

    public Guid? ProductId { get; set; }
    public Product? Product { get; set; }

    public string MaterialName { get; set; } = string.Empty;
    public decimal QuantityUsed { get; set; }
    public string Unit { get; set; } = string.Empty;

    public bool IsRecycled { get; set; }
    public bool IsRenewable { get; set; }
    public bool IsLocallySourced { get; set; }
    public bool IsBiodegradable { get; set; }
    public decimal CarbonSavingsPerUnitKg { get; set; }

    public DateTime RecordedAt { get; set; }
}

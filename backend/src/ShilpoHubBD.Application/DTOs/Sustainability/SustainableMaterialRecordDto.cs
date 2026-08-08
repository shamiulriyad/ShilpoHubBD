namespace ShilpoHubBD.Application.DTOs.Sustainability;

public class SustainableMaterialRecordDto
{
    public Guid Id { get; set; }
    public Guid? ProductId { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public decimal QuantityUsed { get; set; }
    public string Unit { get; set; } = string.Empty;
    public bool IsRecycled { get; set; }
    public bool IsRenewable { get; set; }
    public bool IsLocallySourced { get; set; }
    public bool IsBiodegradable { get; set; }
    public decimal CarbonSavingsPerUnitKg { get; set; }
    public decimal TotalCarbonSavingsKg { get; set; }
    public DateTime RecordedAt { get; set; }
}

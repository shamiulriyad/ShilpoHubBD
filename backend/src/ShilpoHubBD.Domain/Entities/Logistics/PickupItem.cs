namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>A single line on a <see cref="PickupRequest"/> — one package or grouping to be collected.</summary>
public class PickupItem
{
    public Guid Id { get; set; }

    public Guid PickupRequestId { get; set; }
    public PickupRequest PickupRequest { get; set; } = null!;

    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;

    public decimal? WeightKg { get; set; }
    public decimal? LengthCm { get; set; }
    public decimal? WidthCm { get; set; }
    public decimal? HeightCm { get; set; }

    /// <summary>Optional producer SKU / order-item reference.</summary>
    public string? Reference { get; set; }

    public bool IsFragile { get; set; }
}

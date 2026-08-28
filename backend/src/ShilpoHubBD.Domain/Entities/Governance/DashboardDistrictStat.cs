using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.Governance;

/// <summary>
/// Per-district figures captured as part of a <see cref="NationalDashboardSnapshot"/>. Used to render
/// district rankings for the period without recomputing from the transactional tables.
/// </summary>
public class DashboardDistrictStat
{
    public Guid Id { get; set; }

    public Guid NationalDashboardSnapshotId { get; set; }
    public NationalDashboardSnapshot Snapshot { get; set; } = null!;

    public Guid DistrictId { get; set; }
    public District District { get; set; } = null!;

    /// <summary>Denormalised so rankings survive a district rename/deactivation.</summary>
    public string DistrictName { get; set; } = string.Empty;
    public string Division { get; set; } = string.Empty;

    public int ProducerCount { get; set; }
    public int ProductCount { get; set; }
    public int VillageCount { get; set; }
    public int OrderCount { get; set; }
    public decimal SalesValue { get; set; }

    /// <summary>1-based rank by <see cref="SalesValue"/> within the snapshot.</summary>
    public int Rank { get; set; }
}

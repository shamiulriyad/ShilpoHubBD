using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Governance;

/// <summary>
/// A captured point-in-time set of national heritage-economy metrics, produced by a Government / NGO
/// user. Snapshots make period-over-period trend and growth analysis possible; the live overview is
/// always computed on demand and never stored here.
/// </summary>
public class NationalDashboardSnapshot
{
    public Guid Id { get; set; }

    /// <summary>Human label for the snapshot, e.g. "August 2026" or "FY 2025-26".</summary>
    public string Label { get; set; } = string.Empty;

    public DashboardPeriod Period { get; set; }

    /// <summary>Inclusive start of the window the metrics were computed over.</summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>Exclusive end of the window the metrics were computed over.</summary>
    public DateTime PeriodEnd { get; set; }

    public DateTime CapturedAt { get; set; }

    // ---- Producers ----------------------------------------------------------
    public int TotalProducers { get; set; }
    public int ActiveProducers { get; set; }
    public int VerifiedHeritageProducers { get; set; }
    public int NewProducers { get; set; }

    // ---- Employment -------------------------------------------------------
    public int JobsPosted { get; set; }
    public int JobApplications { get; set; }
    public int JobsFilled { get; set; }

    // ---- Export growth --------------------------------------------------
    public int ExporterPartners { get; set; }
    public int ExportOrders { get; set; }
    public decimal ExportSalesValue { get; set; }

    // ---- Heritage economy ---------------------------------------------
    public int TotalOrders { get; set; }
    public int ProductsSold { get; set; }
    public decimal MarketplaceSalesValue { get; set; }
    public decimal HeritageEconomyValue { get; set; }

    // ---- Tourism ------------------------------------------------------
    public int TourismBookings { get; set; }
    public decimal TourismRevenue { get; set; }
    public int TouristsServed { get; set; }

    // ---- Coverage ---------------------------------------------------
    public int DistrictsCovered { get; set; }
    public int VillagesCovered { get; set; }
    public int ProductsListed { get; set; }

    public string? Notes { get; set; }

    public Guid GeneratedByUserId { get; set; }
    public User GeneratedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<DashboardDistrictStat> DistrictStats { get; set; } = new List<DashboardDistrictStat>();
}

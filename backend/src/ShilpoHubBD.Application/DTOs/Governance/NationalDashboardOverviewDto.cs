namespace ShilpoHubBD.Application.DTOs.Governance;

/// <summary>Live, on-demand snapshot of national heritage-economy health. Never persisted.</summary>
public class NationalDashboardOverviewDto
{
    public DateTime GeneratedAt { get; set; }

    /// <summary>Start of the window the figures cover; null means all-time.</summary>
    public DateTime? FromDate { get; set; }

    /// <summary>End of the window the figures cover; null means "up to now".</summary>
    public DateTime? ToDate { get; set; }

    public ProducerMetricsDto Producers { get; set; } = new();
    public EmploymentMetricsDto Employment { get; set; } = new();
    public ExportGrowthMetricsDto ExportGrowth { get; set; } = new();
    public TourismMetricsDto Tourism { get; set; } = new();
    public HeritageEconomyMetricsDto HeritageEconomy { get; set; } = new();
    public CoverageMetricsDto Coverage { get; set; } = new();
}

public class ProducerMetricsDto
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int VerifiedHeritage { get; set; }
    public int NewInWindow { get; set; }
}

public class EmploymentMetricsDto
{
    public int JobsPosted { get; set; }
    public int ActiveJobListings { get; set; }
    public int JobApplications { get; set; }
    public int JobsFilled { get; set; }
    public double FillRatePercent { get; set; }
}

public class ExportGrowthMetricsDto
{
    public int ExporterPartners { get; set; }
    public int ExportOrders { get; set; }
    public decimal ExportSalesValue { get; set; }

    /// <summary>Export sales for the immediately preceding window of equal length; null if no window was given.</summary>
    public decimal? PreviousExportSalesValue { get; set; }

    /// <summary>Percent change vs the preceding window; null if it cannot be computed.</summary>
    public double? GrowthPercent { get; set; }
}

public class TourismMetricsDto
{
    public int Bookings { get; set; }
    public int CompletedBookings { get; set; }
    public decimal TourismRevenue { get; set; }
    public int TouristsServed { get; set; }
    public int ActiveServices { get; set; }
}

public class HeritageEconomyMetricsDto
{
    public decimal MarketplaceSalesValue { get; set; }
    public decimal TourismRevenue { get; set; }
    public decimal TotalValue { get; set; }
    public int OrdersPlaced { get; set; }
    public int ProductsSold { get; set; }
    public decimal AverageOrderValue { get; set; }
}

public class CoverageMetricsDto
{
    public int DistrictsWithProducers { get; set; }
    public int TotalDistricts { get; set; }
    public int Villages { get; set; }
    public int ProductsListed { get; set; }
}

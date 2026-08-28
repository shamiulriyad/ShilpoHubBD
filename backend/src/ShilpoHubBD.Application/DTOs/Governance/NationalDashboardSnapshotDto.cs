namespace ShilpoHubBD.Application.DTOs.Governance;

public class NationalDashboardSnapshotDto
{
    public Guid Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime CapturedAt { get; set; }

    public int TotalProducers { get; set; }
    public int ActiveProducers { get; set; }
    public int VerifiedHeritageProducers { get; set; }
    public int NewProducers { get; set; }

    public int JobsPosted { get; set; }
    public int JobApplications { get; set; }
    public int JobsFilled { get; set; }

    public int ExporterPartners { get; set; }
    public int ExportOrders { get; set; }
    public decimal ExportSalesValue { get; set; }

    public int TotalOrders { get; set; }
    public int ProductsSold { get; set; }
    public decimal MarketplaceSalesValue { get; set; }
    public decimal HeritageEconomyValue { get; set; }

    public int TourismBookings { get; set; }
    public decimal TourismRevenue { get; set; }
    public int TouristsServed { get; set; }

    public int DistrictsCovered { get; set; }
    public int VillagesCovered { get; set; }
    public int ProductsListed { get; set; }

    public string? Notes { get; set; }
    public Guid GeneratedByUserId { get; set; }
    public string? GeneratedByName { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<DashboardDistrictStatDto> DistrictStats { get; set; } = new();
}

public class DashboardDistrictStatDto
{
    public Guid DistrictId { get; set; }
    public string DistrictName { get; set; } = string.Empty;
    public string Division { get; set; } = string.Empty;
    public int ProducerCount { get; set; }
    public int ProductCount { get; set; }
    public int VillageCount { get; set; }
    public int OrderCount { get; set; }
    public decimal SalesValue { get; set; }
    public int Rank { get; set; }
}

public class NationalDashboardSnapshotListItemDto
{
    public Guid Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime CapturedAt { get; set; }
    public int TotalProducers { get; set; }
    public decimal HeritageEconomyValue { get; set; }
    public decimal ExportSalesValue { get; set; }
    public string? GeneratedByName { get; set; }
}

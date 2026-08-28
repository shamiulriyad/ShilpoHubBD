using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Application.Services.Governance;

internal static class GovernanceMappings
{
    public static NationalDashboardSnapshotDto ToDto(this NationalDashboardSnapshot s) => new()
    {
        Id = s.Id,
        Label = s.Label,
        Period = s.Period.ToString(),
        PeriodStart = s.PeriodStart,
        PeriodEnd = s.PeriodEnd,
        CapturedAt = s.CapturedAt,
        TotalProducers = s.TotalProducers,
        ActiveProducers = s.ActiveProducers,
        VerifiedHeritageProducers = s.VerifiedHeritageProducers,
        NewProducers = s.NewProducers,
        JobsPosted = s.JobsPosted,
        JobApplications = s.JobApplications,
        JobsFilled = s.JobsFilled,
        ExporterPartners = s.ExporterPartners,
        ExportOrders = s.ExportOrders,
        ExportSalesValue = s.ExportSalesValue,
        TotalOrders = s.TotalOrders,
        ProductsSold = s.ProductsSold,
        MarketplaceSalesValue = s.MarketplaceSalesValue,
        HeritageEconomyValue = s.HeritageEconomyValue,
        TourismBookings = s.TourismBookings,
        TourismRevenue = s.TourismRevenue,
        TouristsServed = s.TouristsServed,
        DistrictsCovered = s.DistrictsCovered,
        VillagesCovered = s.VillagesCovered,
        ProductsListed = s.ProductsListed,
        Notes = s.Notes,
        GeneratedByUserId = s.GeneratedByUserId,
        GeneratedByName = s.GeneratedBy?.FullName,
        CreatedAt = s.CreatedAt,
        DistrictStats = s.DistrictStats
            .OrderBy(d => d.Rank)
            .Select(d => new DashboardDistrictStatDto
            {
                DistrictId = d.DistrictId,
                DistrictName = d.DistrictName,
                Division = d.Division,
                ProducerCount = d.ProducerCount,
                ProductCount = d.ProductCount,
                VillageCount = d.VillageCount,
                OrderCount = d.OrderCount,
                SalesValue = d.SalesValue,
                Rank = d.Rank,
            })
            .ToList(),
    };

    public static NationalDashboardSnapshotListItemDto ToListItemDto(this NationalDashboardSnapshot s) => new()
    {
        Id = s.Id,
        Label = s.Label,
        Period = s.Period.ToString(),
        PeriodStart = s.PeriodStart,
        PeriodEnd = s.PeriodEnd,
        CapturedAt = s.CapturedAt,
        TotalProducers = s.TotalProducers,
        HeritageEconomyValue = s.HeritageEconomyValue,
        ExportSalesValue = s.ExportSalesValue,
        GeneratedByName = s.GeneratedBy?.FullName,
    };
}

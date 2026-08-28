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

    public static HeritageIndexRecordDto ToDto(this HeritageIndexRecord r) => new()
    {
        Id = r.Id,
        IndexType = r.IndexType.ToString(),
        Scope = r.Scope.ToString(),
        ScopeId = r.ScopeId,
        ScopeLabel = r.ScopeLabel,
        Score = r.Score,
        Rating = r.Rating.ToString(),
        Method = r.Method,
        Summary = r.Summary,
        PeriodStart = r.PeriodStart,
        PeriodEnd = r.PeriodEnd,
        ComputedAt = r.ComputedAt,
        Notes = r.Notes,
        GeneratedByUserId = r.GeneratedByUserId,
        GeneratedByName = r.GeneratedBy?.FullName,
        CreatedAt = r.CreatedAt,
        Components = r.Components
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new HeritageIndexComponentDto
            {
                Key = c.Key,
                Label = c.Label,
                RawValue = c.RawValue,
                Weight = c.Weight,
                ContributionScore = c.ContributionScore,
                Detail = c.Detail,
                DisplayOrder = c.DisplayOrder,
            })
            .ToList(),
    };

    public static HeritageIndexRecordListItemDto ToListItemDto(this HeritageIndexRecord r) => new()
    {
        Id = r.Id,
        IndexType = r.IndexType.ToString(),
        Scope = r.Scope.ToString(),
        ScopeId = r.ScopeId,
        ScopeLabel = r.ScopeLabel,
        Score = r.Score,
        Rating = r.Rating.ToString(),
        PeriodStart = r.PeriodStart,
        PeriodEnd = r.PeriodEnd,
        ComputedAt = r.ComputedAt,
        GeneratedByName = r.GeneratedBy?.FullName,
    };

    public static PolicySimulationDto ToDto(this PolicySimulation s) => new()
    {
        Id = s.Id,
        Title = s.Title,
        SimulationType = s.SimulationType.ToString(),
        Scope = s.Scope.ToString(),
        ScopeId = s.ScopeId,
        ScopeLabel = s.ScopeLabel,
        Status = s.Status.ToString(),
        HorizonMonths = s.HorizonMonths,
        InputsJson = s.InputsJson,
        AssumptionsJson = s.AssumptionsJson,
        Method = s.Method,
        Summary = s.Summary,
        Confidence = s.Confidence.ToString(),
        BaselineProducers = s.BaselineProducers,
        BaselineActiveProducers = s.BaselineActiveProducers,
        BaselineEmployment = s.BaselineEmployment,
        BaselineExportValue = s.BaselineExportValue,
        BaselineTourismRevenue = s.BaselineTourismRevenue,
        BaselineEconomyValue = s.BaselineEconomyValue,
        Notes = s.Notes,
        FailureReason = s.FailureReason,
        GeneratedByUserId = s.GeneratedByUserId,
        GeneratedByName = s.GeneratedBy?.FullName,
        CreatedAt = s.CreatedAt,
        CompletedAt = s.CompletedAt,
        Projections = s.Projections
            .OrderBy(p => p.DisplayOrder)
            .Select(p => new PolicySimulationProjectionDto
            {
                Metric = p.Metric,
                Unit = p.Unit,
                BaselineValue = p.BaselineValue,
                ProjectedValue = p.ProjectedValue,
                DeltaValue = p.DeltaValue,
                DeltaPercent = p.DeltaPercent,
                HorizonMonths = p.HorizonMonths,
                Confidence = p.Confidence.ToString(),
                Detail = p.Detail,
                DisplayOrder = p.DisplayOrder,
            })
            .ToList(),
        Recommendations = s.Recommendations
            .OrderBy(r => r.DisplayOrder)
            .Select(r => new PolicySimulationRecommendationDto
            {
                Priority = r.Priority.ToString(),
                Title = r.Title,
                Detail = r.Detail,
                DisplayOrder = r.DisplayOrder,
            })
            .ToList(),
    };

    public static PolicySimulationListItemDto ToListItemDto(this PolicySimulation s) => new()
    {
        Id = s.Id,
        Title = s.Title,
        SimulationType = s.SimulationType.ToString(),
        Scope = s.Scope.ToString(),
        ScopeLabel = s.ScopeLabel,
        Status = s.Status.ToString(),
        HorizonMonths = s.HorizonMonths,
        Confidence = s.Confidence.ToString(),
        CreatedAt = s.CreatedAt,
        GeneratedByName = s.GeneratedBy?.FullName,
    };
}

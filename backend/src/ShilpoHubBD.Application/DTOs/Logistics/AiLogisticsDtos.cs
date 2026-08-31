namespace ShilpoHubBD.Application.DTOs.Logistics;

public class AiLogisticsQueryParameters
{
    public Guid? ShipmentId { get; set; }
    public Guid? DeliveryRouteId { get; set; }
    public string? Scope { get; set; }
    public string? Status { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

// ---- Delivery prediction ---------------------------------------------

public class DeliveryPredictionDto
{
    public Guid Id { get; set; }
    public Guid LogisticsPartnerProfileId { get; set; }
    public Guid ShipmentId { get; set; }
    public string? ShipmentTrackingNumber { get; set; }
    public Guid GeneratedByUserId { get; set; }
    public string? GeneratedByName { get; set; }
    public string Method { get; set; } = string.Empty;
    public DateTime? PredictedDeliveryAt { get; set; }
    public double PredictedTransitDays { get; set; }
    public double OnTimeProbability { get; set; }
    public double PredictedFailureProbability { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public string Confidence { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? FactorsJson { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DeliveryPredictionListItemDto
{
    public Guid Id { get; set; }
    public Guid ShipmentId { get; set; }
    public string? ShipmentTrackingNumber { get; set; }
    public DateTime? PredictedDeliveryAt { get; set; }
    public double OnTimeProbability { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public string Confidence { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// ---- Demand forecast -----------------------------------------------

public class DemandForecastDto
{
    public Guid Id { get; set; }
    public Guid LogisticsPartnerProfileId { get; set; }
    public Guid GeneratedByUserId { get; set; }
    public string? GeneratedByName { get; set; }
    public string Scope { get; set; } = string.Empty;
    public Guid? ScopeId { get; set; }
    public string ScopeLabel { get; set; } = string.Empty;
    public string Metric { get; set; } = string.Empty;
    public int HorizonDays { get; set; }
    public string Granularity { get; set; } = "day";
    public string Method { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Confidence { get; set; } = string.Empty;
    public double BaselineDailyAverage { get; set; }
    public double PredictedTotal { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string? AssumptionsJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<DemandForecastPointDto> Points { get; set; } = new();
}

public class DemandForecastPointDto
{
    public DateTime PeriodDate { get; set; }
    public double PredictedValue { get; set; }
    public double LowerBound { get; set; }
    public double UpperBound { get; set; }
}

public class DemandForecastListItemDto
{
    public Guid Id { get; set; }
    public string Scope { get; set; } = string.Empty;
    public string ScopeLabel { get; set; } = string.Empty;
    public string Metric { get; set; } = string.Empty;
    public int HorizonDays { get; set; }
    public double PredictedTotal { get; set; }
    public string Confidence { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// ---- Route optimization run --------------------------------------

public class RouteOptimizationRunDto
{
    public Guid Id { get; set; }
    public Guid LogisticsPartnerProfileId { get; set; }
    public Guid DeliveryRouteId { get; set; }
    public string? DeliveryRouteCode { get; set; }
    public Guid GeneratedByUserId { get; set; }
    public string? GeneratedByName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Objective { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public decimal? OriginalDistanceKm { get; set; }
    public decimal? ProposedDistanceKm { get; set; }
    public decimal? DistanceSavingKm { get; set; }
    public int? ProposedDurationMinutes { get; set; }
    public string Confidence { get; set; } = string.Empty;
    public DateTime? AppliedAt { get; set; }
    public Guid? AppliedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<RouteOptimizationRunStopDto> Stops { get; set; } = new();
}

public class RouteOptimizationRunStopDto
{
    public Guid RouteStopId { get; set; }
    public int OriginalSequence { get; set; }
    public int ProposedSequence { get; set; }
    public decimal? DistanceFromPreviousKm { get; set; }
    public string Label { get; set; } = string.Empty;
}

public class RouteOptimizationRunListItemDto
{
    public Guid Id { get; set; }
    public Guid DeliveryRouteId { get; set; }
    public string? DeliveryRouteCode { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Objective { get; set; } = string.Empty;
    public decimal? DistanceSavingKm { get; set; }
    public string Confidence { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// ---- Warehouse allocation recommendation ---------------------

public class WarehouseAllocationRecommendationDto
{
    public Guid Id { get; set; }
    public Guid LogisticsPartnerProfileId { get; set; }
    public Guid GeneratedByUserId { get; set; }
    public string? GeneratedByName { get; set; }
    public string Objective { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public int? Quantity { get; set; }
    public bool RequireColdChain { get; set; }
    public Guid? DestinationDistrictId { get; set; }
    public string? DestinationDistrictName { get; set; }
    public Guid? ShipmentId { get; set; }
    public string? ShipmentTrackingNumber { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Confidence { get; set; } = string.Empty;
    public Guid? RecommendedWarehouseId { get; set; }
    public string? RecommendedWarehouseCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<WarehouseAllocationOptionDto> Options { get; set; } = new();
}

public class WarehouseAllocationOptionDto
{
    public Guid WarehouseId { get; set; }
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public int Rank { get; set; }
    public double Score { get; set; }
    public double ProjectedUtilizationPercent { get; set; }
    public bool SameDistrictAsDestination { get; set; }
    public string Rationale { get; set; } = string.Empty;
}

public class WarehouseAllocationRecommendationListItemDto
{
    public Guid Id { get; set; }
    public string Objective { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public Guid? RecommendedWarehouseId { get; set; }
    public string? RecommendedWarehouseCode { get; set; }
    public string Confidence { get; set; } = string.Empty;
    public int OptionCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

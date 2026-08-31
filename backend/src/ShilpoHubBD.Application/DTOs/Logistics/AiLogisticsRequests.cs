namespace ShilpoHubBD.Application.DTOs.Logistics;

public class PredictDeliveryRequest
{
    public Guid ShipmentId { get; set; }

    /// <summary>How many days of history to draw lane / partner stats from. Default 120.</summary>
    public int? LookbackDays { get; set; }

    /// <summary>When false, the prediction is returned but not stored (Id will be empty).</summary>
    public bool Persist { get; set; } = true;
}

public class OptimizeRouteAiRequest
{
    public Guid DeliveryRouteId { get; set; }

    /// <summary>proximity, balanced, capacity, coldchain or cost. Default proximity.</summary>
    public string? Objective { get; set; }

    public double? AverageSpeedKmh { get; set; }

    public bool Persist { get; set; } = true;
}

public class ForecastDemandRequest
{
    /// <summary>Network, District or Warehouse. Default Network.</summary>
    public string? Scope { get; set; }

    /// <summary>District id or warehouse id when scope is District / Warehouse.</summary>
    public Guid? ScopeId { get; set; }

    /// <summary>Network / District: shipments, pickups, returns or weight_kg. Warehouse: inbound or outbound.</summary>
    public string? Metric { get; set; }

    public int HorizonDays { get; set; } = 14;

    /// <summary>day or week. Default day.</summary>
    public string? Granularity { get; set; }

    /// <summary>History window in days. Default 90.</summary>
    public int? LookbackDays { get; set; }

    public bool Persist { get; set; } = true;
}

public class RecommendWarehouseRequest
{
    /// <summary>balanced, proximity, capacity, coldchain or cost. Default balanced.</summary>
    public string? Objective { get; set; }

    public string? Sku { get; set; }
    public int? Quantity { get; set; }
    public Guid? DestinationDistrictId { get; set; }

    /// <summary>When set, the shipment's destination district is used and stored on the recommendation.</summary>
    public Guid? ShipmentId { get; set; }

    public bool RequireColdChain { get; set; }

    public bool Persist { get; set; } = true;
}

using FluentValidation;
using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Application.Validators.Logistics;

public class PredictDeliveryRequestValidator : AbstractValidator<PredictDeliveryRequest>
{
    public PredictDeliveryRequestValidator()
    {
        RuleFor(x => x.ShipmentId).NotEmpty();
        RuleFor(x => x.LookbackDays).InclusiveBetween(14, 365).When(x => x.LookbackDays.HasValue);
    }
}

public class OptimizeRouteAiRequestValidator : AbstractValidator<OptimizeRouteAiRequest>
{
    private static readonly string[] Objectives = { "proximity", "balanced", "capacity", "coldchain", "cost" };

    public OptimizeRouteAiRequestValidator()
    {
        RuleFor(x => x.DeliveryRouteId).NotEmpty();
        RuleFor(x => x.Objective)
            .Must(v => Objectives.Contains(v!.Trim().ToLowerInvariant()))
            .When(x => !string.IsNullOrWhiteSpace(x.Objective))
            .WithMessage("Objective must be one of: proximity, balanced, capacity, coldchain, cost.");
        RuleFor(x => x.AverageSpeedKmh).InclusiveBetween(1, 200).When(x => x.AverageSpeedKmh.HasValue);
    }
}

public class ForecastDemandRequestValidator : AbstractValidator<ForecastDemandRequest>
{
    public ForecastDemandRequestValidator()
    {
        RuleFor(x => x.Scope)
            .Must(v => Enum.TryParse<DemandForecastScope>(v, true, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.Scope))
            .WithMessage("Scope must be one of: Network, District, Warehouse.");
        RuleFor(x => x.Metric)
            .Must(v => new[] { "shipments", "pickups", "returns", "weight_kg", "inbound", "outbound" }
                .Contains(v!.Trim().ToLowerInvariant()))
            .When(x => !string.IsNullOrWhiteSpace(x.Metric))
            .WithMessage("Metric must be one of: shipments, pickups, returns, weight_kg, inbound, outbound.");
        RuleFor(x => x.Granularity)
            .Must(v => v!.Trim().ToLowerInvariant() is "day" or "week")
            .When(x => !string.IsNullOrWhiteSpace(x.Granularity))
            .WithMessage("Granularity must be 'day' or 'week'.");
        RuleFor(x => x.HorizonDays).InclusiveBetween(1, 180);
        RuleFor(x => x.LookbackDays).InclusiveBetween(14, 365).When(x => x.LookbackDays.HasValue);
    }
}

public class RecommendWarehouseRequestValidator : AbstractValidator<RecommendWarehouseRequest>
{
    public RecommendWarehouseRequestValidator()
    {
        RuleFor(x => x.Objective)
            .Must(v => Enum.TryParse<WarehouseAllocationObjective>(v, true, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.Objective))
            .WithMessage("Objective must be one of: Balanced, Proximity, Capacity, ColdChain, Cost.");
        RuleFor(x => x.Sku).MaximumLength(80);
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0).When(x => x.Quantity.HasValue);
    }
}
